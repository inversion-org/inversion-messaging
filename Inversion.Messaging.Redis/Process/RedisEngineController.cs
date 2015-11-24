using System;

using Inversion.Messaging.Extensions;
using Inversion.Messaging.Model;

namespace Inversion.Messaging.Process
{
    public class RedisEngineController : RedisEngineStore, IEngineController
    {
        public RedisEngineController(string connections, int databaseNumber, IMachineNameProvider machineNameProvider, TimeSpan statusExpiry)
            : base(connections, databaseNumber, machineNameProvider, statusExpiry) {}

        public void ReceiveCommand(string name, IEngineCommandReceiver engineCommandReceiver, EngineStatus currentStatus)
        {
            EngineStatus desiredStatus = this.GetDesiredStatus(name);

            if (currentStatus != desiredStatus ||
                (currentStatus != EngineStatus.Paused && desiredStatus == EngineStatus.Paused) ||
                (currentStatus != EngineStatus.Off && desiredStatus == EngineStatus.Off))
            {
                engineCommandReceiver.ProcessControlMessage(new EngineControlStatus
                {
                    CurrentStatus = currentStatus,
                    DesiredStatus = desiredStatus,
                    Updated = DateTime.Now,
                    Name = name
                });
            }
        }

        public void UpdateCurrentStatus(string name, EngineStatus currentStatus)
        {
            this.SetCurrentStatus(name, currentStatus);
        }

        public void UpdateDesiredStatus(string name, EngineStatus desiredStatus)
        {
            this.SetDesiredStatus(name, desiredStatus);
        }

        public void ForceStatus(string name, EngineControlStatus status)
        {
            this.SetCurrentStatus(name, status.CurrentStatus);
            this.SetDesiredStatus(name, status.DesiredStatus);
        }

        public void UpdateGlobalDesiredStatus(string name, EngineStatus desiredStatus)
        {
            this.SetGlobalDesiredStatus(name, desiredStatus);
        }
    }
}