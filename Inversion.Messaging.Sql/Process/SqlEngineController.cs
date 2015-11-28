using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

using Inversion.Data.Store;
using Inversion.Messaging.Extensions;
using Inversion.Messaging.Model;

namespace Inversion.Messaging.Process
{
    public abstract class SqlEngineController : SqlStore, IEngineController
    {
        protected abstract string ReceiveCommandQuery { get; }
        protected abstract string UpdateCurrentStatusQuery { get; }
        protected abstract string UpdateDesiredStatusQuery { get; }
        protected abstract string UpdateGlobalDesiredStatusQuery { get; }
        protected abstract string EnsureControlRowExistsQuery { get; }

        private readonly IMachineNameProvider _machineNameProvider;

        protected SqlEngineController(string connStr, IMachineNameProvider machineNameProvider)
            : base(SqlClientFactory.Instance, connStr)
        {
            _machineNameProvider = machineNameProvider;
        }

        protected SqlEngineController(DbProviderFactory instance, string connStr,
            IMachineNameProvider machineNameProvider) : base(instance, connStr)
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

        protected EngineStatus GetDesiredStatus(string name)
        {
            string globalKey = this.GetGlobalStatusKey(name);
            string machineKey = this.GetMachineStatusKey(name);

            EngineStatus globalStatus = this.GetStatus(globalKey, "Desired");

            if (globalStatus == EngineStatus.Off || globalStatus == EngineStatus.Paused)
            {
                return globalStatus;
            }

            EngineStatus machineStatus = this.GetStatus(machineKey, "Desired");

            return machineStatus;
        }

        protected EngineStatus GetStatus(string name, string columnName)
        {
            using (IDataReader dataReader = this.Read(this.ReceiveCommandQuery, _parameter("@name", name)))
            {
                if (dataReader.Read())
                {
                    return this.ReadEngineStatus(dataReader, columnName);
                }
            }

            return EngineStatus.Null;
        }

        protected EngineStatus ReadEngineStatus(IDataReader dataReader, string columnName)
        {
            return (EngineStatus) dataReader.ReadInt(columnName);
        }

        public void UpdateCurrentStatus(string name, EngineStatus currentStatus)
        {
            string machineKey = this.GetMachineStatusKey(name);

            this.Exec(this.UpdateCurrentStatusQuery,
                _parameter("@name", machineKey),
                _parameter("@currentstatus", (int) currentStatus),
                _parameter("@date", DateTime.Now)
            );
        }

        public void UpdateDesiredStatus(string name, EngineStatus desiredStatus)
        {
            string machineKey = this.GetMachineStatusKey(name);

            this.Exec(this.UpdateDesiredStatusQuery,
                _parameter("@name", machineKey),
                _parameter("@desiredstatus", (int) desiredStatus),
                _parameter("@date", DateTime.Now)
            );
        }

        public void ForceStatus(string name, EngineControlStatus status)
        {
            string machineKey = this.GetMachineStatusKey(name);

            this.EnsureControlRowExists(machineKey, status.CurrentStatus, status.DesiredStatus);
        }

        protected void EnsureControlRowExists(string name, EngineStatus currentStatus, EngineStatus desiredStatus)
        {
            this.Exec(this.EnsureControlRowExistsQuery,
                _parameter("@name", name),
                _parameter("@currentstatus", (int) currentStatus),
                _parameter("@desiredstatus", (int) desiredStatus),
                _parameter("@date", DateTime.Now)
            );
        }

        public void UpdateGlobalDesiredStatus(string name, EngineStatus desiredStatus)
        {
            string globalKey = this.GetGlobalStatusKey(name);

            this.Exec(this.UpdateGlobalDesiredStatusQuery, 
                _parameter("@name", globalKey),
                _parameter("@desiredstatus", (int) desiredStatus),
                _parameter("@date", DateTime.Now)
                );
        }

        protected EngineControlStatus Read(IDataReader dataReader)
        {
            // TODO: get rid of the Enum.Parse as it uses reflection, move to either a switch or a dictionary lookup
            return new EngineControlStatus
            {
                Name = dataReader.ReadString("Name"),
                CurrentStatus = this.ReadEngineStatus(dataReader, "Current"),
                DesiredStatus = this.ReadEngineStatus(dataReader, "Desired"),
                Updated = dataReader.ReadDateTime("Updated")
            };
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