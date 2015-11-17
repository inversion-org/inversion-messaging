using System;

using Inversion.Data.Redis;
using Inversion.Messaging.Extensions;
using Inversion.Messaging.Model;

namespace Inversion.Messaging.Process
{
    public class RedisEngineController : RedisStore, IEngineController
    {
        public RedisEngineController(string connections, int databaseNumber) : base(connections, databaseNumber) { }

        public void ReceiveCommand(string name, IEngineCommandReceiver engineCommandReceiver)
        {
            string currentStatusAsString = this.Database.HashGet(name, "current");
            string desiredStatusAsString = this.Database.HashGet(name, "desired");

            EngineStatus currentStatus = (EngineStatus) Convert.ToInt32(currentStatusAsString);
            EngineStatus desiredStatus = (EngineStatus) Convert.ToInt32(desiredStatusAsString);
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
            this.Database.HashSet(name, "current", ((int) currentStatus).ToString());
            this.Database.HashSet(name, "updated", DateTime.Now.ToString("o"));
        }

        public void ForceStatus(string name, EngineControlStatus status)
        {
            this.Database.HashSet(name, "current", ((int) status.CurrentStatus).ToString());
            this.Database.HashSet(name, "desired", ((int) status.DesiredStatus).ToString());
            this.Database.HashSet(name, "name", name);
            this.Database.HashSet(name, "updated", DateTime.Now.ToString("o"));
        }
    }
}