using System.Data.Common;
using System.Data.SqlClient;

namespace Inversion.Messaging.Process
{
    public class MsSqlEngineController : SqlEngineController
    {
        protected override string ReceiveCommandQuery
        {
            get { return @"
SELECT Name, CurrentStatus, DesiredStatus, Updated
FROM [EventProcessingControl]
WHERE Name = @name
"; }
        }

        protected override string UpdateCurrentStatusQuery
        {
            get { return @"
UPDATE [EventProcessingControl]
SET CurrentStatus = @currentstatus, Updated = @date
WHERE Name = @name
"; }
        }

        protected override string ForceCurrentStatusQuery
        {
            get { return @"
UPDATE [EventProcessingControl]
SET CurrentStatus = @currentstatus, DesiredStatus = @desiredstatus, Updated = @date
WHERE Name = @name
"; }
        }

        public MsSqlEngineController(string connStr) : base(SqlClientFactory.Instance, connStr) {}
        public MsSqlEngineController(DbProviderFactory instance, string connStr) : base(instance, connStr) {}
    }
}