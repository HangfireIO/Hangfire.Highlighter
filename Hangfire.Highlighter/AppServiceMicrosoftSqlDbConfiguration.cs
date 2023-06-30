using System.Data.Entity.SqlServer;
using System.Data.Entity;

namespace Hangfire.Highlighter
{
    public class AppServiceMicrosoftSqlDbConfiguration : DbConfiguration
    {
        const string SystemDataSqlClient = "System.Data.SqlClient";

        public AppServiceMicrosoftSqlDbConfiguration()
        {
            SetProviderFactory(SystemDataSqlClient, Microsoft.Data.SqlClient.SqlClientFactory.Instance);
            SetProviderServices(SystemDataSqlClient, MicrosoftSqlProviderServices.Instance);
            SetExecutionStrategy(SystemDataSqlClient, () => new MicrosoftSqlAzureExecutionStrategy());
        }
    }
}