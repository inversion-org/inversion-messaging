using Inversion.Data;

namespace Inversion.Messaging.Process
{
    public interface IEngineController : IStore
    {
        void ReceiveCommand(string name, IEngineCommandReceiver engineCommandReceiver);
        void UpdateCurrentStatus(string name, EngineStatus currentStatus);
    }
}