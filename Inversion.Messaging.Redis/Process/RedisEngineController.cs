using System;

using StackExchange.Redis;

using Inversion.Data.Redis;
using Inversion.Messaging.Extensions;
using Inversion.Messaging.Model;

namespace Inversion.Messaging.Process
{
    public class RedisEngineController : RedisStore, IEngineController
    {
        public RedisEngineController(string connections, int databaseNumber) : base(connections, databaseNumber) { }

        public RedisEngineController(ConnectionMultiplexer connectionMultiplexer, int databaseNumber) : base(connectionMultiplexer, databaseNumber) {}

        public void ReceiveCommand(string name, IEngineCommandReceiver engineCommandReceiver)
        {
            string controlAsString = this.Database.StringGet(name);

            EngineControlStatus status = controlAsString.FromJSON<EngineControlStatus>();

            if (status.CurrentStatus != status.DesiredStatus ||
                (status.CurrentStatus != EngineStatus.Paused && status.DesiredStatus == EngineStatus.Paused) ||
                (status.CurrentStatus != EngineStatus.Off && status.DesiredStatus == EngineStatus.Off))
            {
                ProcessControlMessage(engineCommandReceiver, status);
            }
        }

        public void UpdateCurrentStatus(string name, EngineStatus currentStatus)
        {
            string controlAsString = this.Database.StringGet(name);

            EngineControlStatus status = controlAsString.FromJSON<EngineControlStatus>();

            status.CurrentStatus = currentStatus;
            status.Updated = DateTime.Now;

            this.Database.StringSet(name, status.ToJSON());
        }

        protected void ProcessControlMessage(IEngineCommandReceiver engineCommandReceiver, EngineControlStatus eventProcessingControl)
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

        public void ForceStatus(string name, EngineControlStatus status)
        {
            this.Database.StringSet(name, status.ToJSON());
        }
    }
}