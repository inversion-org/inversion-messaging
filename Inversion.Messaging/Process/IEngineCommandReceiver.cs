namespace Inversion.Messaging.Process
{
    public interface IEngineCommandReceiver
    {
        void Shutdown();
        void Pause();
        void Resume();
    }
}
