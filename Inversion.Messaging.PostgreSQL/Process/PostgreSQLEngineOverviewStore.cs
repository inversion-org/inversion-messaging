using System;
using System.Data.Common;
using Npgsql;

namespace Inversion.Messaging.Process
{
    public class PostgreSQLEngineOverviewStore : SqlEngineOverviewStore
    {
        protected string BaseQuery
        {
            get { return @"
SELECT ""Name"", ""Current"", ""Desired"", ""Updated"" FROM ""EventProcessingControl"" {0}
"; }
        }
        protected override string GetQuery
        {
            get { return String.Format(this.BaseQuery, @" WHERE ""Name"" = @name"); }
        }

        protected override string GetAllQuery
        {
            get { return String.Format(this.BaseQuery, @" WHERE ""Name"" LIKE '%@%'"); }
        }

        public PostgreSQLEngineOverviewStore(string connStr) : base(NpgsqlFactory.Instance, connStr) { }
        public PostgreSQLEngineOverviewStore(DbProviderFactory instance, string connStr) : base(instance, connStr) { }
    }
}