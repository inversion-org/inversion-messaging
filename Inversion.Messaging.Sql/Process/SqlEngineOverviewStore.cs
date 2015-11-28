using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

using Inversion.Data.Store;
using Inversion.Messaging.Model;

namespace Inversion.Messaging.Process
{
    public abstract class SqlEngineOverviewStore : SqlStore, IEngineOverviewStore
    {
        protected abstract string GetQuery { get; }
        protected abstract string GetAllQuery { get; }

        protected SqlEngineOverviewStore(string connStr) : base(SqlClientFactory.Instance, connStr) { }
        protected SqlEngineOverviewStore(DbProviderFactory instance, string connStr) : base(instance, connStr) { }

        public EngineOverview GetGlobalStatus(string name)
        {
            return this.Get(name);
        }

        public EngineOverview Get(string name)
        {
            using (IDataReader dataReader = this.Read(this.GetQuery, _parameter("@name", name)))
            {
                while (dataReader.Read())
                {
                    return this.Read(dataReader);
                }
            }

            return null;
        }

        public IEnumerable<EngineOverview> GetAll()
        {
            List<EngineOverview> results = new List<EngineOverview>();
            using (IDataReader dataReader = this.Read(this.GetAllQuery))
            {
                while (dataReader.Read())
                {
                    results.Add(this.Read(dataReader));
                }
            }

            return results;
        }

        protected EngineOverview Read(IDataReader dataReader)
        {
            return new EngineOverview.Builder
            {
                Id = dataReader.ReadString("Name").GetHashCode().ToString(),
                Name = dataReader.ReadString("Name"),
                CurrentStatus = (EngineStatus) dataReader.ReadInt("Current"),
                DesiredStatus = (EngineStatus) dataReader.ReadInt("Desired"),
                Updated = dataReader.ReadDateTime("Updated")
            };
        }
    }
}