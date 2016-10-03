using System;
using System.Collections.Generic;

using Inversion.Extensibility.Extensions;
using Inversion.Messaging.Model;
using Inversion.Messaging.Transport;
using Inversion.Process;
using Inversion.Process.Behaviour;

namespace Inversion.Messaging.Process.Behaviour
{
    public class PopEventBehaviour : PrototypedBehaviour
    {
        public PopEventBehaviour(string respondsTo) : base(respondsTo) {}
        public PopEventBehaviour(string respondsTo, IPrototype prototype) : base(respondsTo, prototype) {}
        public PopEventBehaviour(string respondsTo, IEnumerable<IConfigurationElement> config) : base(respondsTo, config) {}

        public override void Action(IEvent ev, IProcessContext context)
        {
            string service = this.Configuration.GetNameWithAssert("config", "service");

            IEvent poppedEvent = null;

            using (IPop pop = context.Services.GetService<IPop>(service))
            {
                pop.Start();

                poppedEvent = pop.Pop();
            }

            if (poppedEvent != null)
            {
                context.Fire(new MessagingEvent(context, ev.Message, DateTime.Now, ev.Params));
            }
        }
    }
}