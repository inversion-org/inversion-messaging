using Inversion.Data;

namespace Inversion.Messaging.Logging
{
    public interface ILoggingStore : IStore
    {
        void Log(string entity, string message);
        void LogDebug(string entity, string message);
    }
}