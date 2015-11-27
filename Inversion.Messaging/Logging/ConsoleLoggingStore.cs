using System;
using Inversion.Data;

namespace Inversion.Messaging.Logging
{
    public class ConsoleLoggingStore : StoreBase, ILoggingStore
    {
        public override void Dispose()
        {
            // nothing to do
        }

        public void Log(string entity, string message)
        {
            Console.WriteLine("{0}-{1:yyyymmdd}:{1:hhMMss}.{1:fff} {2}", entity, DateTime.Now, message);
        }
    }
}