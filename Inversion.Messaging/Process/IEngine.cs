using Inversion.Data;

namespace Inversion.Messaging.Process
{
    public interface IEngine : IStore
    {
        EngineConfiguration Configuration { get; set; }
        EngineStatus CurrentStatus { get; }
        void Join();
        void Pause();
        void Process();
        void Resume();
        void Shutdown();
        long TotalProcessed { get; }
    }
}