using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

using Inversion.Data;
using Inversion.Messaging.Model;
using Inversion.Messaging.Transport;
using Inversion.Process;
using Inversion.Process.Behaviour;

namespace Inversion.Messaging.Process
{
    /// <summary>
    /// The engine.
    /// It is based upon a Store, so it is IDisposable.
    /// The engine is constructed and then Start() should be called to begin processing.
    /// Join() can be used by the caller to block while waiting for the engine to complete its work.
    /// </summary>
    public class Engine : Store, IEngineCommandReceiver, IEngine
    {
        private readonly ITransport _incoming;
        private readonly ITransport _success;
        private readonly ITransport _failure;
        private readonly IEngineController _control;

        private ITargetBlock<EngineStatus> _engineChain;
        private ITargetBlock<EngineStatus> _engineHeartbeat;

        private TransformBlock<IEvent, Tuple<IEvent, bool>> _engineProcessBlock;

        private readonly List<ActionBlock<Tuple<IEvent, bool>>> _enginePushBlocks = new List<ActionBlock<Tuple<IEvent, bool>>>();

        protected List<bool> _readerSucceeded = new List<bool>();
        protected DateTime _mostRecentReaderSuccess = DateTime.MinValue;

        private Task _engineTask;
        private EngineStatus _desiredState;
        private bool _disposed = false;
        private bool _startLatch = false;

        private bool _runControlHandler = false;
        private Task _controlTask;

        private EngineStatus _currentStatus = EngineStatus.Off;
        private long _totalProcessed = 0;
        private bool _drained = false;

        protected string EngineId = String.Format("{0}:{1}", System.Environment.MachineName, Guid.NewGuid());

        public EngineStatus CurrentStatus
        {
            get { return _currentStatus; }
        }

        public long TotalProcessed
        {
            get { return _totalProcessed; }
        }

        /// <summary>
        /// The configuration object. If one is not provided then one is instantiated which will contain default working values.
        /// </summary>
        public EngineConfiguration Configuration
        {
            get { return _config; }
        }

        private readonly EngineConfiguration _config;

        private IServiceContainer _serviceContainer;
        private IResourceAdapter _resourceAdapter;

        public Engine(ITransport incoming, ITransport success, ITransport failure, IEngineController control) :
            this(incoming, success, failure, control, new EngineConfiguration()) { }

        public Engine(ITransport incoming, ITransport success, ITransport failure, IEngineController control, EngineConfiguration configuration)
        {
            _incoming = incoming;
            _success = success;
            _failure = failure;
            _control = control;
            _config = configuration;
        }

        /// <summary>
        /// Starting the engine.
        /// This performs some state set up, starting the transports for incoming/success/fail queues if they have not been started.
        /// After that, the engine task itself is started.
        /// </summary>
        public override void Start()
        {
            _currentStatus = EngineStatus.Starting;

            _desiredState = EngineStatus.Working;

            if (!_incoming.HasStarted)
            {
                _incoming.Start();
            }
            if (!_success.HasStarted)
            {
                _success.Start();
            }
            if (!_failure.HasStarted)
            {
                _failure.Start();
            }
            if (!_control.HasStarted)
            {
                _control.Start();
            }

            base.Start();
        }

        public void Process(IServiceContainer serviceContainer, IResourceAdapter resourceAdapter)
        {
            _serviceContainer = serviceContainer;
            _resourceAdapter = resourceAdapter;

            Initialise();

            _engineTask = Task.Factory.StartNew(Run);
        }

        protected TransformBlock<EngineStatus, IEvent> MakeReadBlock(int index)
        {
            return new TransformBlock<EngineStatus, IEvent>(
                (engineStatus) =>
                {
                    int myIndex = index;
                    if (_currentStatus == EngineStatus.Working)
                    {
                        IEvent result = _incoming.Pop();
                        _readerSucceeded[myIndex] = (result != null);
                        return result;
                    }

                    return null;
                });
        }

        protected void Initialise()
        {
            BroadcastBlock<EngineStatus> broadcastBlock = new BroadcastBlock<EngineStatus>(engineStatus => engineStatus);

            TransformBlock<EngineStatus, IEvent> heartbeatBlock = new TransformBlock<EngineStatus, IEvent>(
                (engineStatus) =>
                {
                    if (_currentStatus == EngineStatus.Working)
                    {
                        return new MessagingEvent(null, "engine::heartbeat", DateTime.Now,
                            new Dictionary<string, string>());
                    }

                    return null;
                });

            TransformBlock<IEvent, Tuple<IEvent, bool>> processBlock = new TransformBlock<IEvent, Tuple<IEvent, bool>>(
                (ev) =>
                {
                    _startLatch = true;

                    //Console.WriteLine(ev.ToJson());

                    Tuple<IEvent, bool> t = new Tuple<IEvent, bool>(ev, ProcessEvent(ev));

                    System.Threading.Interlocked.Increment(ref _totalProcessed);

                    return t;
                });

            ActionBlock<Tuple<IEvent, bool>> successBlock = new ActionBlock<Tuple<IEvent, bool>>(
                (t) =>
                {
                    if (t.Item1.Message != "engine::heartbeat")
                    {
                        //Console.WriteLine("Pushing event to success");
                        _success.Push(t.Item1);
                    }
                });

            ActionBlock<Tuple<IEvent, bool>> failedBlock = new ActionBlock<Tuple<IEvent, bool>>(
                (t) =>
                {
                    //Console.WriteLine("Pushing event to failure");
                    _failure.Push(t.Item1);
                });

            List<TransformBlock<EngineStatus, IEvent>> readers = new List<TransformBlock<EngineStatus, IEvent>>();
            for (int x = 0; x < _config.NumberOfWorkerTasks; x++)
            {
                TransformBlock<EngineStatus, IEvent> reader = this.MakeReadBlock(x);
                broadcastBlock.LinkTo(reader);
                reader.LinkTo(processBlock, ev => ev != null);
                reader.LinkTo(new ActionBlock<IEvent>((ev) => _drained = true));
                readers.Add(reader);
                _readerSucceeded.Add(false);
            }

            heartbeatBlock.LinkTo(processBlock, ev => ev != null);
            heartbeatBlock.LinkTo(DataflowBlock.NullTarget<IEvent>());

            processBlock.LinkTo(successBlock, t => t.Item2);
            processBlock.LinkTo(failedBlock, t => !t.Item2);

            _engineChain = broadcastBlock;
            _engineHeartbeat = heartbeatBlock;

            _engineProcessBlock = processBlock;
            _enginePushBlocks.Add(successBlock);
            _enginePushBlocks.Add(failedBlock);

            StartControlHandler();
        }

        protected void StartControlHandler()
        {
            // give the control a chance to pause us or stop us before we begin processing
            _control.ReceiveCommand(_config.ControlName, this);

            _control.UpdateCurrentStatus(_config.ControlName, _currentStatus);

            SendHeartbeat();

            // now begin the task normally
            _runControlHandler = true;
            _controlTask = Task.Factory.StartNew(ControlHandler, TaskCreationOptions.LongRunning);
        }

        protected void ShutdownControlHandler()
        {
            // request the control handler task to stop
            _runControlHandler = false;

            // wait for it to complete
            _controlTask.Wait();

            // update our status (should be EngineStatus.Stopping)
            _control.UpdateCurrentStatus(_config.ControlName, _currentStatus);
        }

        /// <summary>
        /// Allows an external caller to block and wait for the engine to complete its work.
        /// Probably should be used in conjunction with the ExitOnEmptyQueue config parameter.
        /// </summary>
        public void Join()
        {
            _engineTask.Wait();
        }

        /// <summary>
        /// Stopping the engine.
        /// This performs some state management, changing the desired state of the engine to stopped and then waiting for the task to complete.
        /// After that is done, the transports for incoming, success and fail queues will be stopped if they are in the appropriate state.
        /// </summary>
        public override void Stop()
        {
            _currentStatus = EngineStatus.Stopping;

            // change the engine's desired state to stopped, this will signal the engine task.
            _desiredState = EngineStatus.Off;
            _currentStatus = EngineStatus.Off;

            _control.UpdateCurrentStatus(_config.ControlName, EngineStatus.Off);

            if (_incoming.HasStarted)
            {
                _incoming.Stop();
            }
            if (_success.HasStarted)
            {
                _success.Stop();
            }
            if (_failure.HasStarted)
            {
                _failure.Stop();
            }
            if (_control.HasStarted)
            {
                _control.Stop();
            }

            base.Stop();
        }

        /// <summary>
        /// Disposing resources.
        /// This will perform a stop (which may already have been done) and then call the dispose methods for the incoming, success and fail transports.
        /// </summary>
        public override void Dispose()
        {
            if (!_disposed)
            {
                this.Stop();

                _incoming.Dispose();
                _success.Dispose();
                _failure.Dispose();

                _disposed = true;
            }
        }

        /// <summary>
        /// This is the engine's main setup and event pump task.
        /// </summary>
        protected void Run()
        {
            // set initial engine state
            _currentStatus = EngineStatus.Working;

            // set our loop flag to its initial state
            bool keepRunning = true;

            Console.WriteLine("Engine entering main loop");

            // main event pump loop
            while (keepRunning)
            {
                // determine if we have been switched off from an external source
                keepRunning = (_desiredState != EngineStatus.Off && _desiredState != EngineStatus.Stopping);

                if (_desiredState == EngineStatus.Paused)
                {
                    _currentStatus = EngineStatus.Paused;
                }
                else if (_desiredState == EngineStatus.Working)
                {
                    _currentStatus = EngineStatus.Working;
                }
                else if (_desiredState == EngineStatus.Off)
                {
                    _currentStatus = EngineStatus.Stopping;
                }

                bool processMessages = _currentStatus == EngineStatus.Working;

                if (keepRunning && processMessages)
                {
                    _engineChain.Post(_currentStatus);

                    if (_startLatch && _config.ExitOnEmptyQueue && _drained)
                    {
                        // if we have no events to process but we have done some processing and we have been asked to exit on an empty queue, exit loop
                        _desiredState = EngineStatus.Off;
                        continue;
                    }
                }

                int yieldTime = this.CalculateYieldTime();

                if (keepRunning)
                {
                    // yield for a configurable time
                    System.Threading.Thread.Sleep(yieldTime);
                }
            }

            _engineChain.Complete();
            _engineChain.Completion.Wait();

            bool blocksStillHaveInput = true;
            while (blocksStillHaveInput)
            {
                blocksStillHaveInput = _engineProcessBlock.InputCount > 0 ||
                                       _enginePushBlocks.Any(b => b.InputCount > 0);
                System.Threading.Thread.Sleep(_config.EngineMinimumYieldTime);
            }

            //ShutdownPushHandler();
            ShutdownControlHandler();
        }

        protected virtual int CalculateYieldTime()
        {
            bool anyReaderSuccess = _readerSucceeded.Any(s => s);

            if (anyReaderSuccess)
            {
                _mostRecentReaderSuccess = DateTime.Now;
            }

            if (!anyReaderSuccess)
            {
                if (DateTime.Now.Subtract(_mostRecentReaderSuccess).TotalMilliseconds > _config.EngineCooldownTime)
                {
                    return _config.EngineMaximumYieldTime;
                }
            }

            return _config.EngineMinimumYieldTime;
        }

        private void ControlHandler()
        {
            while (_runControlHandler)
            {
                _control.UpdateCurrentStatus(_config.ControlName, _currentStatus);
                _control.ReceiveCommand(_config.ControlName, this);

                SendHeartbeat();

                // yield thread for the configured number of milliseconds
                System.Threading.Thread.Sleep(_config.ControlHandlerYieldTime);
            }
        }

        private void SendHeartbeat()
        {
            _engineHeartbeat.Post(_currentStatus);
        }

        /// <summary>
        /// Perform the actual Event processing.
        /// This will create a Context for the event, register any behaviours in the event-behaviours list
        /// and then fire a constructed event containing the passed parameters.
        /// If the event completes without exception, the original event will be passed to the queue of events that will be pushed to the success transport.
        /// If the event results in an exception being thrown, the original event will be passed to the queue of events that will be pushed to the fail transport.
        /// </summary>
        /// <param name="e">The event to process</param>
        protected bool ProcessEvent(IEvent e)
        {
            // create a fresh context
            ProcessContext context = new ProcessContext(_serviceContainer, _resourceAdapter);

            //Console.WriteLine("ProcessEvent {0}", e.Message);

            // read the list of behaviours from the 'event-behaviours' list
            IList<IProcessBehaviour> behaviours =
                context.Services.GetService<List<IProcessBehaviour>>("event-behaviours");
            // register them on the context message bus
            context.Register(behaviours);
            // begin an overall timer
            context.Timers.Begin("engine::begin-event");

            bool success = false;

            DateTime eventCreated = DateTime.Now;

            if (e is MessagingEvent)
            {
                eventCreated = ((MessagingEvent) e).Created;
            }

            try
            {
                // construct a new event with our fresh context, the source event's message and parameters
                // then fire the event - this will perform the actual behavioural work
                IEvent thisEvent = new MessagingEvent(context, e.Message, eventCreated, e.Params).Fire();

                // escalate any errors
                if (thisEvent.HasParams("_failed"))
                {
                    string exceptionDetail = String.Join("\n", context.Errors);
                    e.Params.Add("event::exception", exceptionDetail);
                }
                else
                {
                    // retrieve params created by job
                    foreach (KeyValuePair<string, string> kvp in context.Params)
                    {
                        thisEvent.Add(kvp.Key, kvp.Value);
                    }

                    success = true;
                }
            }
            catch (Exception ex)
            {
                // capture the exception details in the event parameters
                string exceptionDetail = ex.ToString();
                e.Params.Add("event::exception", exceptionDetail);
            }
            finally
            {
                // perform house-keeping on the context
                context.Completed();
            }

            return success;
        }

        /// <summary>
        /// Callback from our control handler
        /// </summary>
        public void Shutdown()
        {
            Console.WriteLine("Engine: Shutting Down");
            Trace.TraceInformation("Engine: Shutting Down");
            _desiredState = EngineStatus.Off;
        }

        public void Pause()
        {
            Console.WriteLine("Engine: Pausing");
            Trace.TraceInformation("Engine: Pausing");
            _desiredState = EngineStatus.Paused;
        }

        public void Resume()
        {
            Console.WriteLine("Engine: Resuming");
            Trace.TraceInformation("Engine: Resuming");
            _desiredState = EngineStatus.Working;
        }

        public void EnsureStarted()
        {
            _control.ForceStatus(_config.ControlName,
                new EngineControlStatus
            {
                CurrentStatus = EngineStatus.Starting,
                DesiredStatus = EngineStatus.Working,
                Name = _config.ControlName,
                Updated = DateTime.Now
            });
        }
    }
}