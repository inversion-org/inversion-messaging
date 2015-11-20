using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

using Inversion.Data.Store;
using Inversion.Messaging.Model;

namespace Inversion.Messaging.Process
{
    public abstract class SqlEngineController : SqlStore, IEngineController
    {
        protected abstract string ReceiveCommandQuery { get; }
        protected abstract string UpdateCurrentStatusQuery { get; }
        protected abstract string ForceCurrentStatusQuery { get; }

        protected SqlEngineController(string connStr) : base(SqlClientFactory.Instance, connStr) { }
        protected SqlEngineController(DbProviderFactory instance, string connStr) : base(instance, connStr) { }

        public void ReceiveCommand(string name, IEngineCommandReceiver engineCommandReceiver, EngineStatus currentStatus)
        {
            // TODO: change mechanism to use the passed current status from engine.
            using (IDataReader dataReader = this.Read(this.ReceiveCommandQuery, _parameter("@name", name)))
            {
                if (dataReader.Read())
                {
                    EngineControlStatus control = this.Read(dataReader);

                    if (control.CurrentStatus != control.DesiredStatus ||
                        (control.CurrentStatus != EngineStatus.Paused && control.DesiredStatus == EngineStatus.Paused) ||
                        (control.CurrentStatus != EngineStatus.Off && control.DesiredStatus == EngineStatus.Off)
                    )
                    {
                        ProcessControlMessage(engineCommandReceiver, control);
                    }
                }
            }
        }

        public void UpdateCurrentStatus(string name, EngineStatus currentStatus)
        {
            this.Exec(this.UpdateCurrentStatusQuery,
                _parameter("@name", name),
                _parameter("@currentstatus", currentStatus.ToString().ToLower()),
                _parameter("@date", DateTime.Now)
            );
        }

        public void UpdateDesiredStatus(string name, EngineStatus desiredStatus)
        {
            throw new NotImplementedException();
        }

        public void ForceStatus(string name, EngineControlStatus status)
        {
            this.Exec(this.ForceCurrentStatusQuery,
                _parameter("@name", name),
                _parameter("@currentstatus", status.CurrentStatus.ToString().ToLower()),
                _parameter("@desiredstatus", status.DesiredStatus.ToString().ToLower()),
                _parameter("@date", DateTime.Now)
            );
        }

        protected EngineControlStatus Read(IDataReader dataReader)
        {
            // TODO: get rid of the Enum.Parse as it uses reflection, move to either a switch or a dictionary lookup
            return new EngineControlStatus
            {
                Name = dataReader.ReadString("Name"),
                CurrentStatus = (EngineStatus)Enum.Parse(typeof(EngineStatus), dataReader.ReadString("CurrentStatus"), true),
                DesiredStatus = (EngineStatus)Enum.Parse(typeof(EngineStatus), dataReader.ReadString("DesiredStatus"), true),
                Updated = dataReader.ReadDateTime("Updated")
            };
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
    }
}