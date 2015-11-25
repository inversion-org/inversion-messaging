using System;

using Inversion.Data;

namespace Inversion.Messaging.Process
{
    public interface ILoggingStore : IStore
    {
        void Log(string entity, string message);
    }
}