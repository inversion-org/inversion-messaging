using System;

using Inversion.Data;
using Inversion.Messaging.Extensions;
using Inversion.Messaging.Model;

namespace Inversion.Messaging.Process
{
    public class DynamoDBEngineController : DynamoDBStore, IEngineController
    {
        public DynamoDBEngineController(string serviceUrl, string accessKey, string accessSecret) : base(serviceUrl, accessKey, accessSecret) {}

        public void ReceiveCommand(string name, IEngineCommandReceiver engineCommandReceiver, EngineStatus currentStatus)
        {
            // TODO: modify this to use the passed current status of engine
            DynamoDBEngineControlStatus source = this.Context.Load<DynamoDBEngineControlStatus>(name);

            EngineControlStatus status = source.ToModel();

            if (status.CurrentStatus != status.DesiredStatus ||
                (status.CurrentStatus != EngineStatus.Paused && status.DesiredStatus == EngineStatus.Paused) ||
                (status.CurrentStatus != EngineStatus.Off && status.DesiredStatus == EngineStatus.Off))
            {
                engineCommandReceiver.ProcessControlMessage(status);
            }
        }

        public void UpdateCurrentStatus(string name, EngineStatus currentStatus)
        {
            DynamoDBEngineControlStatus source = this.Context.Load<DynamoDBEngineControlStatus>(name);

            EngineControlStatus status = source.ToModel();

            status.CurrentStatus = currentStatus;
            status.Updated = DateTime.Now;

            this.Context.Save<DynamoDBEngineControlStatus>(new DynamoDBEngineControlStatus(status));
        }

        public void UpdateDesiredStatus(string name, EngineStatus desiredStatus)
        {
            throw new NotImplementedException();
        }

        public void ForceStatus(string name, EngineControlStatus status)
        {
            if (status.Name != name)
            {
                throw new ArgumentException("Name in engine control status was different to passed argument.");
            }

            this.Context.Save<DynamoDBEngineControlStatus>(new DynamoDBEngineControlStatus(status));
        }

        public void UpdateGlobalDesiredStatus(string name, EngineStatus desiredStatus)
        {
            throw new NotImplementedException();
        }
    }
}