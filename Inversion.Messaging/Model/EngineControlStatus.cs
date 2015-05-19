using System;

using Inversion.Messaging.Process;

namespace Inversion.Messaging.Model
{
    public class EngineControlStatus
    {
        public string Name { get; set; }
        public EngineStatus CurrentStatus { get; set; }
        public EngineStatus DesiredStatus { get; set; }
        public DateTime Updated { get; set; }
    }
}