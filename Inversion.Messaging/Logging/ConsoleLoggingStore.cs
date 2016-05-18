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
            Console.WriteLine(String.Format("{0} {1}: {2}", DateTime.Now, entity, message));
        }

        public void LogDebug(string entity, string message)
        {
            Console.WriteLine(String.Format("{0} {1}: {2}", DateTime.Now, entity, message));
        }
    }
}