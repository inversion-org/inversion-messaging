using Inversion.Data;
using Inversion.Process;

namespace Inversion.Messaging.Process
{
    public interface IEngine : IStore
    {
        EngineConfiguration Configuration { get; }
        EngineStatus CurrentStatus { get; }
        void Join();
        void Pause();
        void Process(IServiceContainer serviceContainer, IResourceAdapter resourceAdapter);
        void Resume();
        void Shutdown();
        long TotalProcessed { get; }
        void EnsureStarted();
    }
}