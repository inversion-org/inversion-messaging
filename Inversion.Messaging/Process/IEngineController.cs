using Inversion.Data;
using Inversion.Messaging.Model;

namespace Inversion.Messaging.Process
{
    public interface IEngineController : IStore
    {
        void ReceiveCommand(string name, IEngineCommandReceiver engineCommandReceiver);
        void UpdateCurrentStatus(string name, EngineStatus currentStatus);
        void ForceStatus(string name, EngineControlStatus status);
    }
}