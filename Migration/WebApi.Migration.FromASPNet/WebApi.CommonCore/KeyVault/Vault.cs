using System;
using System.Linq;
using System.Reflection;
using WebApi.CommonCore.Helper;
using WebApi.CommonCore.KeyVault.Attributes;

namespace WebApi.CommonCore.KeyVault
{
    public sealed class Vault
    {
        #region Singleton
        static readonly Lazy<Vault> vault = new Lazy<Vault>(() => new Vault());
        public static Vault Current => vault.Value;
        private readonly VaultBase _client;
        private readonly CoreVaultBase _coreVaultBase;

        public CoreVault Core { get; } = new CoreVault();
        private Vault()
        {
            _client = new VaultBase();
            _coreVaultBase = new CoreVaultBase();
            try
            {
                Initialize();
            }
            catch (AggregateException aex)
            {
                throw aex?.InnerException ?? aex.Flatten()?.InnerException ?? aex;
            }

        }
        #endregion

        #region Methods

        private void Initialize(object obj, VaultBase client)
        {
            var properties = obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var property in properties.Where(p => p.CanWrite))
            {
                property.SetValue(obj, client.GetSecret(property).Value);
            }
        }
        private void Initialize()
        {
            Initialize(this, this._client);
            try
            {
                Initialize(this.Core, this._coreVaultBase);
            }
            catch
            {
            }
        }

        public void ReloadVault()
        {
            lock (Current)
            {
                Initialize();
            }
        }
        #endregion

        const string SITE_NAME_MARK = "{SiteName}";
        const string DB_SERVER_NAME_MARK = "{DBConnectionServerName}";

        public string AESKeyBLOB { get; set; }
        public string CustomExportQueue { get; set; }
        public string CreateCustomExportQueue(string siteName)
        {
            return CustomExportQueue?.Replace(SITE_NAME_MARK, siteName);
        }

        public string CustomExportBlobContainer { get; set; }
        public string CreateCustomExportBlobContainer(string siteName)
        {
            return CustomExportBlobContainer?.Replace(SITE_NAME_MARK, siteName);
        }
        public string SendGridApiKey { get; set; }

        #region Sql server
        [VariousSuffixKeySet(SuffixKeySet.Sql)]
        public string DBConnectionServerName { get; set; }
        public string DBConnectionUserName { get; set; }
        public string DBConnectionPassword { get; set; }
        public string DBConnectionStringTemplate { get; set; }
        public string CreateCustomerDbConnectionString(string siteName)
        {
            return DBConnectionStringTemplate?.Replace(SITE_NAME_MARK, siteName).Replace(DB_SERVER_NAME_MARK, DBConnectionServerName);
        }
        #endregion

        #region Storage
        [VariousSuffixKeySet(SuffixKeySet.Storage)]
        public string StorageAccountName { get; set; }
        [VariousSuffixKeySet(SuffixKeySet.Storage)]
        public string StorageAccountKey { get; set; }
        [NonVault]
        public string StorageConnectionString
        => $"DefaultEndpointsProtocol=https;AccountName={StorageAccountName};AccountKey={StorageAccountKey}";

        public string LogStorageConnectionString { get; set; }
        #endregion

        #region service bus
        [VariousSuffixKeySet(SuffixKeySet.ServiceBus)]
        public string ServiceBusNamespace { get; set; }
        [VariousSuffixKeySet(SuffixKeySet.ServiceBus)]
        public string ServiceBusAccessKey { get; set; }
        [VariousSuffixKeySet(SuffixKeySet.ServiceBus)]
        public string ServiceBusAccessKeyName { get; set; }
        [NonVault]
        public string ServiceBusConnectionString
            => $"Endpoint=sb://{ServiceBusNamespace}.servicebus.windows.net/;SharedAccessKeyName={ServiceBusAccessKeyName};SharedAccessKey={ServiceBusAccessKey}";

        #endregion

        public class CoreVault
        {
            public string AESSecretKey { get; set; }
            public string IOConnectorAESKeyBLOB { get; set; }
            public string DBConnectionServerName { get; set; }
            public string DBConnectionUserName { get; set; }
            public string DBConnectionPassword { get; set; }
            public string DBConnectionDBName { get; set; }
            public string StorageAccountNameShared { get; set; }
            public string SharedApiVCardWebForWexEndpoint { get; set; }
            public string SharedApiVCardWebForWexKey { get; set; }
            /// <summary>
            /// Encrypted
            /// </summary>
            public string StorageAccountKeyShared { get; set; }
            [NonVault]
            public string StorageSharedConnectionString
        => $"DefaultEndpointsProtocol=https;AccountName={StorageAccountNameShared};AccountKey={AESHelper.DescryptAES(StorageAccountKeyShared, AESSecretKey)}";
        }
    }
}
