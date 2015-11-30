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
        protected abstract string GetAllGlobalStatusQuery { get; }
        protected abstract string GetEngineQuery { get; }
        protected abstract string GetAllEnginesByControlQuery { get; }
        protected abstract string RemoveQuery { get; }

        protected SqlEngineOverviewStore(string connStr) : base(SqlClientFactory.Instance, connStr) { }
        protected SqlEngineOverviewStore(DbProviderFactory instance, string connStr) : base(instance, connStr) { }

        public IEnumerable<EngineOverview> GetAllGlobalStatus()
        {
            List<EngineOverview> results = new List<EngineOverview>();
            using (IDataReader dataReader = this.Read(this.GetAllGlobalStatusQuery))
            {
                while (dataReader.Read())
                {
                    results.Add(this.Read(dataReader));
                }
            }

            return results;
        }

        public EngineOverview GetGlobalStatus(string name)
        {
            return this.GetEngine(name);
        }

        public EngineOverview GetEngine(string name)
        {
            using (IDataReader dataReader = this.Read(this.GetEngineQuery, _parameter("@name", name)))
            {
                while (dataReader.Read())
                {
                    return this.Read(dataReader);
                }
            }

            return null;
        }

        public IEnumerable<EngineOverview> GetAllEnginesByControl(string name)
        {
            List<EngineOverview> results = new List<EngineOverview>();
            using (IDataReader dataReader = this.Read(this.GetAllEnginesByControlQuery, _parameter("@name", name)))
            {
                while (dataReader.Read())
                {
                    results.Add(this.Read(dataReader));
                }
            }

            return results;
        }

        public void Remove(string name)
        {
            this.Exec(this.RemoveQuery, _parameter("@name", name));
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