using System;
using System.Data.Common;
using Npgsql;

namespace Inversion.Messaging.Process
{
    public class PostgreSQLEngineOverviewStore : SqlEngineOverviewStore
    {
        protected string BaseQuery
        {
            get { return @"SELECT ""Name"", ""Current"", ""Desired"", ""Updated"" FROM ""EventProcessingControl"" {0}"; }
        }

        protected override string GetAllGlobalStatusQuery
        {
            get { return String.Format(this.BaseQuery, @" WHERE ""Name"" NOT LIKE 'engine:%@%'"); }
        }

        protected override string GetEngineQuery
        {
            get { return String.Format(this.BaseQuery, @" WHERE ""Name"" = @name"); }
        }

        protected override string GetAllEnginesByControlQuery
        {
            get { return String.Format(this.BaseQuery, @" WHERE ""Name"" LIKE 'engine:' || @name || '@%'"); }
        }

        protected override string RemoveQuery
        {
            get { return @"DELETE FROM ""EventProcessingControl"" WHERE ""Name"" = @name"; }
        }

        public PostgreSQLEngineOverviewStore(string connStr) : base(NpgsqlFactory.Instance, connStr) { }
        public PostgreSQLEngineOverviewStore(DbProviderFactory instance, string connStr) : base(instance, connStr) { }
    }
}