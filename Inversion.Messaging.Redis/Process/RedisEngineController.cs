using System;

using Inversion.Data.Redis;
using Inversion.Messaging.Extensions;
using Inversion.Messaging.Model;
using StackExchange.Redis;

namespace Inversion.Messaging.Process
{
    public class RedisEngineController : RedisStore, IEngineController
    {
        private readonly IMachineNameProvider _machineNameProvider;

        public RedisEngineController(string connections, int databaseNumber, IMachineNameProvider machineNameProvider)
            : base(connections, databaseNumber)
        {
            _machineNameProvider = machineNameProvider;
        }

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

        protected EngineStatus GetCurrentStatus(string name)
        {
            string key = this.GetMachineStatusKey(name);

            return (EngineStatus) Convert.ToInt32(this.Database.HashGet(key, "current"));
        }

        protected EngineStatus GetDesiredStatus(string name)
        {
            string globalKey = this.GetGlobalStatusKey(name);
            string machineKey = this.GetMachineStatusKey(name);

            EngineStatus globalStatus = (EngineStatus) Convert.ToInt32(this.Database.HashGet(globalKey, "desired"));

            if (globalStatus == EngineStatus.Off || globalStatus == EngineStatus.Paused)
            {
                return globalStatus;
            }

            EngineStatus machineStatus = (EngineStatus)Convert.ToInt32(this.Database.HashGet(machineKey, "desired"));

            return machineStatus;
        }

        protected void SetCurrentStatus(string name, EngineStatus status)
        {
            string key = this.GetMachineStatusKey(name);

            this.Database.HashSet(key,
                new HashEntry[]
                {
                    new HashEntry("current", ((int) status).ToString()),
                    new HashEntry("updated", DateTime.Now.ToString("o"))
                });
        }

        protected void SetDesiredStatus(string name, EngineStatus status)
        {
            string machineKey = this.GetMachineStatusKey(name);

            this.Database.HashSet(machineKey, "desired", ((int) status).ToString());
        }

        protected string GetMachineStatusKey(string name)
        {
            string machineName = _machineNameProvider.Get() ?? Environment.MachineName;
            return String.Format("engine:{0}@{1}", name, machineName);
        }

        protected string GetGlobalStatusKey(string name)
        {
            return name;
        }
    }
}