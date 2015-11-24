using Inversion.Data;
using Inversion.Messaging.Model;

namespace Inversion.Messaging.Process
{
    public interface IEngineController : IStore
    {
        void ReceiveCommand(string name, IEngineCommandReceiver engineCommandReceiver, EngineStatus currentStatus);
        void UpdateCurrentStatus(string name, EngineStatus currentStatus);
        void UpdateDesiredStatus(string name, EngineStatus desiredStatus);
        void ForceStatus(string name, EngineControlStatus status);
        void UpdateGlobalDesiredStatus(string name, EngineStatus desiredStatus);
    }
}