using System.Data.Common;
using System.Data.SqlClient;

namespace Inversion.Messaging.Transport
{
    public class MsSqlTransport : SqlTransport
    {
        public MsSqlTransport(string connStr) : base(SqlClientFactory.Instance, connStr) { }
        public MsSqlTransport(DbProviderFactory instance, string connStr) : base(instance, connStr) { }

        protected override string CountIncomingQuery
        {
            get
            {
                return @"
SELECT COUNT([ID]) FROM Events_Incoming
";
            }
        }

        protected override string CountSuccessQuery
        {
            get
            {
                return @"
SELECT COUNT([ID]) FROM Events_Success
";
            }
        }

        protected override string CountFailedQuery
        {
            get
            {
                return @"
SELECT COUNT([ID]) FROM Events_Failed
";
            }
        }

        protected override string PushToIncomingQuery
        {
            get
            {
                return @"
INSERT INTO Events_Incoming
([ID], [Created], [Modified], [Name], [Parameters])
VALUES
(newid(), getdate(), null, @name, @parameters)
";
            }
        }

        protected override string PushToSuccessQuery
        {
            get
            {
                return @"
INSERT INTO Events_Success
([ID], [Created], [Modified], [Name], [Parameters])
VALUES
(newid(), @created, null, @name, @parameters)
";
            }
        }

        protected override string PushToFailedQuery
        {
            get
            {
                return @"
INSERT INTO Events_Failed
([ID], [Created], [Modified], [Name], [Parameters])
VALUES
(newid(), @created, null, @name, @parameters)
";
            }
        }

        protected override string PopQuery
        {
            get
            {
                return @"
SET NOCOUNT ON;

WITH cte AS (
    SELECT TOP (1) [ID], [Created], [Modified], [Name], [Parameters]
    FROM Events_Incoming
    ORDER BY [Created]
)
DELETE FROM cte
OUTPUT deleted.[ID], deleted.[Created], deleted.[Modified], deleted.[Name], deleted.[Parameters]

";
            }
        }

        protected override string PeekQuery
        {
            get
            {
                return @"
SELECT TOP 1 [ID], [Created], [Modified], [Name], [Parameters]
FROM Events_Incoming
ORDER BY [Created]
";
            }
        }
    }
}