using System;

using Inversion.Data;
using Inversion.Process;

namespace Inversion.Messaging.Transport
{
    public class NullTransport : StoreBase, ITransport
    {
        public override void Dispose()
        {
            // nothing to do
        }

        public IEvent Peek()
        {
            return null;
        }

        public long Count()
        {
            return 0;
        }

        public IEvent Pop()
        {
            return null;
        }

        public void Push(IEvent ev)
        {
            // nothing to do
        }
    }
}