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

        protected override string UpdateDesiredStatusQuery
        {
            get { return @"
UPDATE [EventProcessingControl]
SET DesiredStatus = @desiredstatus, Updated = @date
WHERE Name = @name
"; }
        }

        protected override string UpdateGlobalDesiredStatusQuery { get { return this.UpdateDesiredStatusQuery; } }

        protected override string EnsureControlRowExistsQuery
        {
            get { return @"
DECLARE @c AS int
SELECT @c = COUNT(Name) FROM [EventProcessingControl] WHERE Name = @name
IF @c = 0 BEGIN
    INSERT INTO [EventProcessingControl] (Name, CurrentStatus, DesiredStatus, Updated)
    VALUES (@name, @currentstatus, @desiredstatus, @date)
END
ELSE
BEGIN
    UPDATE [EventProcessingControl]
    SET
        CurrentStatus = @currentstatus,
        DesiredStatus = @desiredstatus,
        Updated = @date
    WHERE Name = @name
END

SELECT SCOPE_IDENTITY()
"; }
        }

        public MsSqlEngineController(string connStr, IMachineNameProvider machineNameProvider) : base(SqlClientFactory.Instance, connStr, machineNameProvider) {}
        public MsSqlEngineController(DbProviderFactory instance, string connStr, IMachineNameProvider machineNameProvider) : base(instance, connStr, machineNameProvider) {}
    }
}