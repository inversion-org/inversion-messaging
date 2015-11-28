using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace Inversion.Messaging.Process
{
    public class PostgreSQLEngineController : SqlEngineController
    {
        protected override string ReceiveCommandQuery
        {
            get { return @"SELECT ""Name"", ""Current"", ""Desired"", ""Updated"" FROM ""EventProcessingControl"" WHERE ""Name"" = @name"; }
        }

        protected override string UpdateCurrentStatusQuery
        {
            get { return @"
UPDATE ""EventProcessingControl""
SET
    ""Current"" = @currentstatus,
    ""Updated"" = @date
WHERE ""Name"" = @name"; }
        }

        protected override string UpdateDesiredStatusQuery
        {
            get { return @"
UPDATE ""EventProcessingControl""
SET
    ""Desired"" = @desiredstatus,
    ""Updated"" = @date
WHERE ""Name"" = @name"; }
        }

        protected override string UpdateGlobalDesiredStatusQuery
        {
            get { return this.UpdateDesiredStatusQuery; }
        }

        protected override string EnsureControlRowExistsQuery
        {
            get { return @"
WITH new_values (""Name"", ""Current"", ""Desired"", ""Updated"") as (
  values 
     (@name, @currentstatus, @desiredstatus, @date)
),
upsert as
( 
    update ""EventProcessingControl"" m 
        set ""Name"" = nv.""Name"",
            ""Current"" = nv.""Current"",
            ""Desired"" = nv.""Desired"",
            ""Updated"" = nv.""Updated""
    FROM new_values nv
    WHERE m.""Name"" = nv.""Name""
    RETURNING m.*
)
INSERT INTO ""EventProcessingControl"" (""Name"", ""Current"", ""Desired"", ""Updated"")
SELECT ""Name"", ""Current"", ""Desired"", ""Updated""
FROM new_values
WHERE NOT EXISTS (SELECT 1 
                  FROM upsert up 
                  WHERE up.""Name"" = new_values.""Name"")
"; }
        }

        public PostgreSQLEngineController(string connStr, IMachineNameProvider machineNameProvider) : base(NpgsqlFactory.Instance, connStr, machineNameProvider) { }
        public PostgreSQLEngineController(DbProviderFactory instance, string connStr, IMachineNameProvider machineNameProvider) : base(instance, connStr, machineNameProvider) { }
    }
}
