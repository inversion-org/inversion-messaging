using System;
using System.Reactive.Linq;

using Inversion.Data;
using Inversion.Process;
using Inversion.Process.Behaviour;

namespace Inversion.Messaging.Process.Context
{
    public class TimedContext : ProcessContext
    {
        public TimedContext(IServiceContainer services, IResourceAdapter resources) : base(services, resources) {}

        public override IEvent Fire(IEvent ev)
        {
            ev.Context.Timers.Begin(ev.Message);
            IEvent result = base.Fire(ev);
            ev.Context.Timers.End(ev.Message);
            return result;
        }

        public override void Register(IProcessBehaviour behaviour)
        {
            this.Bus.Where(behaviour.Condition).Subscribe(
                (IEvent ev) => {
                    string behaviourName = behaviour.GetType().FullName;
                    try
                    {
                        ev.Context.Timers.Begin(behaviourName);

                        behaviour.Action(ev);

                        ev.Context.Timers.End(behaviourName);
                    }
                    catch (Exception err)
                    {
                        behaviour.Rescue(ev, err);
                        throw err;
                    }
                }
            );
        }
    }
}