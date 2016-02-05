using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Newtonsoft.Json;

using Inversion.Process;
using Newtonsoft.Json.Linq;

namespace Inversion.Messaging.Model
{
    public class MessagingEvent : Event
    {
        private readonly DateTime _created;
        public DateTime Created
        {
            get { return _created; }
        }

        /// <summary>
        /// Instantiates a new event bound to a context.
        /// </summary>
        /// <param name="context">The context to which the event is bound.</param>
        /// <param name="message">The simple message the event represents.</param>
        /// <param name="created">The temporal part of the event.</param>
        /// <param name="parameters">The parameters of the event.</param>
        public MessagingEvent(IProcessContext context, string message, DateTime created, IDictionary<string, string> parameters)
            : this(context, message, null, created, parameters) {}

        /// <summary>
        /// Instantiates a new event bound to a context.
        /// </summary>
        /// <param name="context">The context to which the event is bound.</param>
        /// <param name="message">The simple message the event represents.</param>
        /// <param name="obj">An object being carried by the event.</param>
        /// <param name="created">The temporal part of the event.</param>
        /// <param name="parameters">The parameters of the event.</param>
        public MessagingEvent(IProcessContext context, string message, IData obj, DateTime created,
            IDictionary<string, string> parameters)
            : base(context, message, obj, parameters)
        {
            _created = created;
        }

		/// <summary>
		/// Instantiates a new event bound to a context.
		/// </summary>
		/// <param name="context">The context to which the event is bound.</param>
		/// <param name="message">The simple message the event represents.</param>
        /// <param name="created">The temporal part of the event.</param>
		/// <param name="parms">
		/// A sequnce of context parameter names that should be copied from the context
		/// to this event.
		/// </param>
		public MessagingEvent(IProcessContext context, string message, DateTime created, params string[] parms) : this(context, message, null, created, parms) { }

        /// <summary>
        /// Instantiates a new event bound to a context.
        /// </summary>
        /// <param name="context">The context to which the event is bound.</param>
        /// <param name="message">The simple message the event represents.</param>
        /// <param name="obj">An object being carried by the event.</param>
        /// <param name="created">The temporal part of the event.</param>
        /// <param name="parms">
        /// A sequnce of context parameter names that should be copied from the context
        /// to this event.
        /// </param>
        public MessagingEvent(IProcessContext context, string message, IData obj, DateTime created,
            params string[] parms) : base(context, message, obj, parms)
        {
            _created = created;
        }

		/// <summary>
		/// Instantiates a new event as a copy of the event provided.
		/// </summary>
		/// <param name="ev">The event to copy for this new instance.</param>
        public MessagingEvent(IEvent ev) : base(ev) {}

        public MessagingEvent(MessagingEvent ev) : base(ev)
        {
            _created = ev.Created;
        }

        /// <summary>
        /// Creates a string representation of the event.
        /// </summary>
        /// <returns>Returns a new string representing the event.</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("(event @message {0}:{1:o}\n", this.Message, this.Created);
            foreach (KeyValuePair<string, string> entry in this.Params)
            {
                sb.AppendFormat("   ({0} -{1})\n", entry.Key, entry.Value);
            }
            sb.AppendLine(")");

            return sb.ToString();
        }

        /// <summary>
        /// Produces an xml representation of the model.
        /// </summary>
        /// <param name="xml">The writer to used to write the xml to. </param>
        public override void ToXml(XmlWriter xml)
        {
            xml.WriteStartElement("event");
            xml.WriteAttributeString("message", this.Message);
            xml.WriteAttributeString("created", this.Created.ToString("o"));
            xml.WriteStartElement("params");
            foreach (KeyValuePair<string, string> entry in this.Params)
            {
                xml.WriteStartElement("item");
                xml.WriteAttributeString("name", entry.Key);
                xml.WriteAttributeString("value", entry.Value);
                xml.WriteEndElement();
            }
            xml.WriteEndElement();
            xml.WriteEndElement();
        }

        /// <summary>
        /// Produces a json respresentation of the model.
        /// </summary>
        /// <param name="json">The writer to use for producing json.</param>
        public override void ToJson(JsonWriter json)
        {
            json.WriteStartObject();
            json.WritePropertyName("_type");
            json.WriteValue("event");
            json.WritePropertyName("_created");
            json.WriteValue(this.Created.ToString("o"));
            json.WritePropertyName("message");
            json.WriteValue(this.Message);
            json.WritePropertyName("params");
            json.WriteStartObject();
            foreach (KeyValuePair<string, string> kvp in this.Params)
            {
                json.WritePropertyName(kvp.Key);
                json.WriteValue(kvp.Value);
            }
            json.WriteEndObject();
            json.WriteEndObject();
        }

        /// <summary>
        /// Creates a new event from an xml representation.
        /// </summary>
        /// <param name="context">The context to which the new event will be bound.</param>
        /// <param name="xml">The xml representation of an event.</param>
        /// <returns>Returns a new event.</returns>
        public new static Event FromXml(IProcessContext context, string xml)
        {
            try
            {
                XElement ev = XElement.Parse(xml);
                if (ev.Name == "event")
                {
                    return new MessagingEvent(
                        context,
                        ev.Attribute("message").Value,
                        DateTime.Parse(ev.Attribute("created").Value),
                        ev.Elements().ToDictionary(el => el.Attribute("name").Value, el => el.Attribute("value").Value)
                    );
                }
                else
                {
                    throw new ParseException("The expressed type of the json provided does not appear to be an event.");
                }
            }
            catch (Exception err)
            {
                throw new ParseException("An unexpected error was encoutered parsing the provided json into an event object.", err);
            }
        }

        /// <summary>
        /// Creates a new event from an json representation.
        /// </summary>
        /// <param name="context">The context to which the new event will be bound.</param>
        /// <param name="json">The json representation of an event.</param>
        /// <returns>Returns a new event.</returns>
        public new static Event FromJson(IProcessContext context, string json)
        {
            try
            {
                JsonReader reader = new JsonTextReader(new StringReader(json));
                reader.DateParseHandling = DateParseHandling.None;
                JObject job = JObject.Load(reader);

                if (job.Value<string>("_type") == "event")
                {
                    return new MessagingEvent(
                        context,
                        job.Value<string>("message"),
                        DateTime.Parse(job.Value<string>("_created")),
                        job.Value<JObject>("params").Properties().ToDictionary(p => p.Name, p => p.Value.ToString())
                    );
                }
                else
                {
                    throw new ParseException("The expressed type of the json provided does not appear to be an event.");
                }
            }
            catch (Exception err)
            {
                throw new ParseException("An unexpected error was encoutered parsing the provided json into an event object.", err);
            }
        }
    }
}