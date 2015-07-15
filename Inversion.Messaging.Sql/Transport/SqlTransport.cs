using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using Inversion.Data.Store;
using Inversion.Messaging.Extensions;
using Inversion.Messaging.Model;
using Inversion.Process;

namespace Inversion.Messaging.Transport
{
    public abstract class SqlTransport : SqlStore, ITransport
    {

        // eventually we'll move queries to a dictionary rather than dedicated members
        // the current shape of this isn't right
        private string[] _allowedQueues = { "incoming", "success", "failure" };
        private string _queueName;
        protected abstract string CountIncomingQuery { get; }
        protected abstract string CountSuccessQuery { get; }
        protected abstract string CountFailedQuery { get; }
        protected abstract string PushToIncomingQuery { get; }
        protected abstract string PushToSuccessQuery { get; }
        protected abstract string PushToFailedQuery { get; }

        protected abstract string PopQuery { get; }
        protected abstract string PeekQuery { get; }

        public string[] AllowedQueues
        {
            get { return _allowedQueues; }
            set { _allowedQueues = value; }
        }

        public string QueueName
        {
            get { return _queueName; }
            set
            {
                if (!this.AllowedQueues.Contains(value))
                {
                    throw new ApplicationException(String.Format("Unrecognised queue name {0}", value));
                }
                _queueName = value;
            }
        }

        private int _popLockTimeout = 10000;
        public int PopLockTimeout { get { return _popLockTimeout; } set { _popLockTimeout = value; } }


        protected SqlTransport(DbProviderFactory instance, string connStr) : base(instance, connStr) { }

        public long Count()
        {
            string query = String.Empty;
            switch (this.QueueName)
            {
                case "incoming": query = this.CountIncomingQuery; break;
                case "success": query = this.CountSuccessQuery; break;
                case "failure": query = this.CountFailedQuery; break;
            }
            object result = this.Scalar(query);
            return Convert.ToInt64(result.ToString());
        }

        public void Push(IEvent e)
        {
            string query = String.Empty;
            switch (this.QueueName)
            {
                case "incoming": query = this.PushToIncomingQuery; break;
                case "success": query = this.PushToSuccessQuery; break;
                case "failure": query = this.PushToFailedQuery; break;
            }

            DateTime created = DateTime.Now;

            if (e is MessagingEvent)
            {
                created = ((MessagingEvent) e).Created;
            }

            this.Exec(query, this.ConvertEventToParameters(e, created));
        }

        public IEvent Pop()
        {
            using (IDataReader dataReader = this.Read(this.PopQuery,
                _parameter("@timeout", _popLockTimeout)))
            {
                while (dataReader.Read())
                {
                    return this.ReadEvent(dataReader);
                }
            }
            return null;
        }

        public IEvent Peek()
        {
            using (IDataReader dataReader = this.Read(this.PeekQuery))
            {
                while (dataReader.Read())
                {
                    return this.ReadEvent(dataReader);
                }
            }
            return null;
        }

        protected MessagingEvent ReadEvent(IDataReader dataReader)
        {
            return new MessagingEvent(null,
                dataReader.ReadString("Name"),
                dataReader.ReadDateTime("Created"),
                dataReader.ReadString("Parameters").FromJSON<Dictionary<string, string>>());
        }

        protected IDbDataParameter[] ConvertEventToParameters(IEvent e, DateTime created)
        {
            List<IDbDataParameter> parameters = new List<IDbDataParameter> { _parameter("@name", e.Message) };

            parameters.Add(_parameter("@created", created));

            string jsonParameters = e.Params.ToJSON();
            parameters.Add(_parameter("@parameters", jsonParameters));

            return parameters.ToArray();
        }
    }
}