using WebApi.CommonCore.Helper;
using WebApi.CommonCore.KeyVault;

namespace WebApi.StamfordCore.Models.Data
{
    public class DbServiceFactory
    {
        internal static BaseDao GetDao(string sitename)
        {
            string connectionString = Vault.Current.CreateCustomerDbConnectionString(sitename);
            return new BaseDao(connectionString, Vault.Current.DBConnectionUserName, Vault.Current.DBConnectionPassword);
        }
        internal static BaseDao GetCurrent()
        {
            var siteName = ConfigHelper.GetString("SiteName");
            return GetDao(siteName);
        }

        internal static BaseDao GetDao(string dbName, string userName, string password)
        {
            string connectionString = Vault.Current.CreateCustomerDbConnectionString(dbName);
            return new BaseDao(connectionString, userName, password);
        }

    }
}