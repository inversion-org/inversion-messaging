using Inversion.Messaging.Model;
using Inversion.Messaging.Process;

namespace Inversion.Messaging.Extensions
{
    public static class IEngineCommandReceiverEx
    {
        public static void ProcessControlMessage(this IEngineCommandReceiver engineCommandReceiver, EngineControlStatus eventProcessingControl)
        {
            switch (eventProcessingControl.DesiredStatus)
            {
                case EngineStatus.Off: engineCommandReceiver.Shutdown();
                    break;
                case EngineStatus.Paused: engineCommandReceiver.Pause();
                    break;
                case EngineStatus.Working: engineCommandReceiver.Resume();
                    break;
            }
        }
    }
}