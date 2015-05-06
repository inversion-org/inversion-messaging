using Inversion.Data;
using Inversion.Process;

namespace Inversion.Messaging.Process
{
    /// <summary>
    /// Configuration holding class.
    /// Working defaults are provided for all configuration parameters.
    /// </summary>
    public class EngineConfiguration
    {
        /// <summary>
        /// How many worker tasks to start within the engine. Default is 4.
        /// </summary>
        public int NumberOfWorkerTasks { get { return _numberOfWorkerTasks; } set { _numberOfWorkerTasks = value; } }
        private int _numberOfWorkerTasks = 4;

        /// <summary>
        /// How long a worker should idle for if it has no work to perform (milliseconds). Default is 1ms.
        /// </summary>
        public int WorkerIdleTime { get { return _workerIdleTime; } set { _workerIdleTime = value; } }
        private int _workerIdleTime = 1;

        /// <summary>
        /// How long a worker should yield for after it has performed some work (milliseconds). Default is 0ms.
        /// </summary>
        public int WorkerYieldTime { get { return _workerYieldTime; } set { _workerYieldTime = value; } }
        private int _workerYieldTime = 0;

        /// <summary>
        /// How long the engine task should yield for if it cannot find a free worker for an event (milliseconds). Default is 1ms.
        /// </summary>
        public int EngineWaitYieldTime { get { return _engineWaitYieldTime; } set { _engineWaitYieldTime = value; } }
        private int _engineWaitYieldTime = 1;

        /// <summary>
        /// How long the engine task should yield for in-between pumping events (milliseconds). Default is 1ms.
        /// </summary>
        public int EngineYieldTime { get { return _engineYieldTime; } set { _engineYieldTime = value; } }
        private int _engineYieldTime = 1;

        /// <summary>
        /// How long the push handler should yield for in-between work (milliseconds). Default is 1ms.
        /// </summary>
        public int PushHandlerYieldTime { get { return _pushHandlerYieldTime; } set { _pushHandlerYieldTime = value; } }
        private int _pushHandlerYieldTime = 1;

        /// <summary>
        /// Maximum number of events should be pushed to their respective target transports by the push handler at a time. Default is 5.
        /// </summary>
        public int PushHandlerMaxEvents { get { return _pushHandlerMaxEvents; } set { _pushHandlerMaxEvents = value; } }
        private int _pushHandlerMaxEvents = 5;

        /// <summary>
        /// How long the control handler should yield for in-between attempting to receive commands (milliseconds). Default is 1000ms.
        /// </summary>
        public int ControlHandlerYieldTime { get { return _controlHandlerYieldTime; } set { _controlHandlerYieldTime = value; } }
        private int _controlHandlerYieldTime = 1000;

        /// <summary>
        /// Name of the control element
        /// </summary>
        public string ControlName { get { return _controlName; } set { _controlName = value; } }
        private string _controlName = "engine";

        /// <summary>
        /// Whether the engine should exit once queue has been emptied (will only be consulted once an event has been processed)
        /// </summary>
        public bool ExitOnEmptyQueue { get { return _exitOnEmptyQueue; } set { _exitOnEmptyQueue = value; } }
        private bool _exitOnEmptyQueue = false;

        public IServiceContainer ServiceContainer { get { return _serviceContainer; } set { _serviceContainer = value; } }
        private IServiceContainer _serviceContainer;

        public IResourceAdapter ResourceAdapter { get { return _resourceAdapter; } set { _resourceAdapter = value; } }
        private IResourceAdapter _resourceAdapter;
    }
}