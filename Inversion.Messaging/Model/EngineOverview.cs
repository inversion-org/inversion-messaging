using System;
using System.Xml;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Inversion.Messaging.Process;

namespace Inversion.Messaging.Model
{
    public class EngineOverview : IData
    {
        private readonly string _Id;
        private readonly string _Name;
        private readonly EngineStatus _CurrentStatus;
        private readonly EngineStatus _DesiredStatus;
        private readonly DateTime _Updated;

        public string Id { get { return _Id; } }
        public string Name { get { return _Name; } }
        public EngineStatus CurrentStatus { get { return _CurrentStatus; } }
        public EngineStatus DesiredStatus { get { return _DesiredStatus; } }
        public DateTime Updated { get { return _Updated; } }

        public EngineOverview(EngineOverview engineoverview)
        {
            this._Id = engineoverview.Id;
            this._Name = engineoverview.Name;
            this._CurrentStatus = engineoverview.CurrentStatus;
            this._DesiredStatus = engineoverview.DesiredStatus;
            this._Updated = engineoverview.Updated;
        }

        public EngineOverview(string id, string name, EngineStatus currentstatus, EngineStatus desiredstatus, DateTime updated)
        {
            this._Id = id;
            this._Name = name;
            this._CurrentStatus = currentstatus;
            this._DesiredStatus = desiredstatus;
            this._Updated = updated;
        }

        public EngineOverview Mutate(Func<Builder, EngineOverview> mutator)
        {
            Builder builder = new Builder(this);
            return mutator(builder);
        }

        public class Builder
        {
            public static implicit operator EngineOverview(Builder builder)
            {
                return builder.ToModel();
            }

            public static implicit operator Builder(EngineOverview model)
            {
                return new Builder(model);
            }

            public string Id { get; set; }
            public string Name { get; set; }
            public EngineStatus CurrentStatus { get; set; }
            public EngineStatus DesiredStatus { get; set; }
            public DateTime Updated { get; set; }

            public Builder()
            { }

            public Builder(EngineOverview engineoverview)
            {
                this.Id = engineoverview.Id;
                this.Name = engineoverview.Name;
                this.CurrentStatus = engineoverview.CurrentStatus;
                this.DesiredStatus = engineoverview.DesiredStatus;
                this.Updated = engineoverview.Updated;
            }

            public Builder(string id, string name, EngineStatus currentstatus, EngineStatus desiredstatus, DateTime updated)
            {
                this.Id = id;
                this.Name = name;
                this.CurrentStatus = currentstatus;
                this.DesiredStatus = desiredstatus;
                this.Updated = updated;
            }

            public EngineOverview ToModel()
            {
                return new EngineOverview(this.Id, this.Name, this.CurrentStatus, this.DesiredStatus, this.Updated);
            }

            public Builder FromModel(EngineOverview engineoverview)
            {
                this.Id = engineoverview.Id;
                this.Name = engineoverview.Name;
                this.CurrentStatus = engineoverview.CurrentStatus;
                this.DesiredStatus = engineoverview.DesiredStatus;
                this.Updated = engineoverview.Updated;
                return this;
            }
        }

        public object Clone()
        {
            return new EngineOverview.Builder(this);
        }

        public void ToXml(XmlWriter writer)
        {
            writer.WriteStartElement("engineoverview");

            writer.WriteElementString("id", this.Id);
            writer.WriteElementString("name", this.Name);
            writer.WriteElementString("currentstatus", ((int) this.CurrentStatus).ToString());
            writer.WriteElementString("currentstatusdisplay", Enum.GetName(typeof(EngineStatus), this.CurrentStatus));
            writer.WriteElementString("desiredstatus", ((int) this.DesiredStatus).ToString());
            writer.WriteElementString("desiredstatusdisplay", Enum.GetName(typeof(EngineStatus), this.DesiredStatus));
            writer.WriteElementString("updated", this.Updated.ToString("o"));

            writer.WriteEndElement();
        }

        public void ToJson(JsonWriter writer)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("id");
            writer.WriteValue(this.Id);
            writer.WritePropertyName("name");
            writer.WriteValue(this.Name);
            writer.WritePropertyName("currentstatus");
            writer.WriteValue(this.CurrentStatus);
            writer.WritePropertyName("currentstatusdisplay");
            writer.WriteValue(Enum.GetName(typeof(EngineStatus), this.CurrentStatus));
            writer.WritePropertyName("desiredstatus");
            writer.WriteValue(this.DesiredStatus);
            writer.WritePropertyName("desiredstatusdisplay");
            writer.WriteValue(Enum.GetName(typeof(EngineStatus), this.DesiredStatus));
            writer.WritePropertyName("updated");
            writer.WriteValue(this.Updated);

            writer.WriteEndObject();
        }

        public JObject Data
        {
            get { return this.ToJsonObject(); }
        }
    }
}