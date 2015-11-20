using System;

using Inversion.Data;
using Inversion.Messaging.Model;

namespace Inversion.Messaging.Process
{
    public class AmazonSQSEngineController : AmazonSQSStore, IEngineController
    {
        public AmazonSQSEngineController(string serviceUrl, string region, string accessKey, string accessSecret) : base(serviceUrl, region, accessKey, accessSecret) {}

        public void ReceiveCommand(string name, IEngineCommandReceiver engineCommandReceiver, EngineStatus currentStatus)
        {
            throw new NotImplementedException();
        }

        public void UpdateCurrentStatus(string name, EngineStatus currentStatus)
        {
            throw new NotImplementedException();
        }

        public void UpdateDesiredStatus(string name, EngineStatus desiredStatus)
        {
            throw new NotImplementedException();
        }

        public void ForceStatus(string name, EngineControlStatus status)
        {
            throw new NotImplementedException();
        }
    }
}