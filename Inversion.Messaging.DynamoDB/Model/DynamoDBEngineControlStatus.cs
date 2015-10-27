using System;

using Amazon.DynamoDBv2.DataModel;

using Inversion.Messaging.Process;

namespace Inversion.Messaging.Model
{
    [DynamoDBTable("engine-control")]
    public class DynamoDBEngineControlStatus
    {
        [DynamoDBHashKey]
        public string Name { get; set; }
        public int CurrentStatus { get; set; }
        public int DesiredStatus { get; set; }
        public DateTime Updated { get; set; }

        public DynamoDBEngineControlStatus() : base() { }

        public DynamoDBEngineControlStatus(EngineControlStatus status)
        {
            this.Name = status.Name;
            this.CurrentStatus = (int) status.CurrentStatus;
            this.DesiredStatus = (int) status.DesiredStatus;
            this.Updated = status.Updated;
        }

        public EngineControlStatus ToModel()
        {
            return new EngineControlStatus
            {
                Name = this.Name,
                CurrentStatus = (EngineStatus) this.CurrentStatus,
                DesiredStatus = (EngineStatus) this.DesiredStatus,
                Updated = this.Updated
            };
        }
    }
}