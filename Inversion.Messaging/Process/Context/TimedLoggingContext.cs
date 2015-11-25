using System;
using System.Reactive.Linq;
using Inversion.Data;
using Inversion.Process;
using Inversion.Process.Behaviour;

namespace Inversion.Messaging.Process.Context
{
    public class TimedLoggingContext : TimedContext
    {
        private ILoggingStore _logger;

        public TimedLoggingContext(IServiceContainer services, IResourceAdapter resources, ILoggingStore logger)
            : base(services, resources)
        {
            _logger = logger;
        }

        public override IEvent Fire(IEvent ev)
        {
            this.Log("fire", ev.Message);
            return base.Fire(ev);
        }

        public override void Register(IProcessBehaviour behaviour)
        {
            this.Bus.Where(behaviour.Condition).Subscribe(
                (IEvent ev) => {
                                   string behaviourName = behaviour.GetType().FullName;
                                   try
                                   {
                                       this.Log("action", behaviourName);

                                       ev.Context.Timers.Begin(behaviourName);

                                       behaviour.Action(ev);

                                       ev.Context.Timers.End(behaviourName);
                                   }
                                   catch (Exception err)
                                   {
                                       this.Log("rescue", String.Format("{0}: {1}", behaviourName, err.Message));
                                       behaviour.Rescue(ev, err);
                                       throw err;
                                   }
                }
                );
        }

        protected void Log(string entity, string message)
        {
            if (_logger != null)
            {
                _logger.Log(entity, message);
            }
        }
    }
}