using System;
using System.Collections.Generic;
using StackExchange.Redis;

using Inversion.Data.Redis;
using Inversion.Process;
using Newtonsoft.Json.Linq;

namespace Inversion.Messaging.Transport
{
    public class RedisTransport : RedisStore, ITransport
    {
        private readonly string _queueName;

        public RedisTransport(string connections, int databaseNumber, string queueName)
            : base(connections, databaseNumber)
        {
            _queueName = queueName;
        }

        public RedisTransport(ConnectionMultiplexer connectionMultiplexer, int databaseNumber) : base(connectionMultiplexer, databaseNumber) {}

        public void Push(IEvent ev)
        {
            this.Database.ListRightPush(_queueName, ev.ToJson());
        }

        public IEvent Pop()
        {
            return this.ConvertDocumentToEvent(this.Database.ListLeftPop(_queueName));
        }

        public IEvent Peek()
        {
            return this.ConvertDocumentToEvent(this.Database.ListGetByIndex(_queueName, 0));
        }

        public long Count()
        {
            return this.Database.ListLength(_queueName);
        }

        protected IEvent ConvertDocumentToEvent(string source)
        {
            return Event.FromJson(null, source);
        }
    }
}
