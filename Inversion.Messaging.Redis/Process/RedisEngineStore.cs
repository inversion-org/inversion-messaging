using System;

using StackExchange.Redis;

using Inversion.Data.Redis;

namespace Inversion.Messaging.Process
{
    public abstract class RedisEngineStore : RedisStore
    {
        private readonly IMachineNameProvider _machineNameProvider;
        private readonly TimeSpan _statusExpiry;

        protected RedisEngineStore(string connections, int databaseNumber, IMachineNameProvider machineNameProvider, TimeSpan statusExpiry)
            : base(connections, databaseNumber)
        {
            _machineNameProvider = machineNameProvider;
            _statusExpiry = statusExpiry;
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
            this.Database.KeyExpire(key, this.GetStatusKeyExpiry());
        }

        protected DateTime GetStatusKeyExpiry()
        {
            return DateTime.Now.Add(_statusExpiry);
        }

        protected void SetDesiredStatus(string name, EngineStatus status)
        {
            string key = this.GetMachineStatusKey(name);

            this.Database.HashSet(key,
                new HashEntry[]
                {
                    new HashEntry("desired", ((int) status).ToString()),
                    new HashEntry("updated", DateTime.Now.ToString("o"))
                });
            this.Database.KeyExpire(key, this.GetStatusKeyExpiry());
        }

        protected void SetGlobalDesiredStatus(string name, EngineStatus status)
        {
            string key = this.GetGlobalStatusKey(name);

            this.Database.HashSet(key,
                new HashEntry[]
                {
                    new HashEntry("desired", ((int) status).ToString()),
                    new HashEntry("updated", DateTime.Now.ToString("o"))
                });
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