using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using WebApi.CommonCore.KeyVault.Attributes;
using WebApi.CommonCore.Helper;

namespace WebApi.CommonCore.KeyVault
{
    internal class VaultBase
    {
        #region Properties

        public KeyVaultClient VaultClient { get; set; }

        public string AzureVaultAddress { get; set; }

        #endregion

        #region Constructors

        public VaultBase()
        {
            VaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(GetToken));
            AzureVaultAddress = $"https://{ConfigHelper.GetString("KeyVault.VaultName")}.vault.azure.net/"; ;//có rồi
        }

        #endregion

        #region Key Vault Implementation

        public SecretBundle GetSecret(PropertyInfo property)
        {
            if (IsVaultProperty(property))
            {
                return VaultClient.GetSecretAsync(AzureVaultAddress, GetVaultKeyName(property)).Result;
            }
            return new SecretBundle { Value = string.Empty };
        }
        public static bool IsVaultProperty(PropertyInfo property)
        {
            return !property.GetCustomAttributes(typeof(NonVaultAttribute)).Any();
        }
        public static string GetVaultKeyName(PropertyInfo property)
        {
            var keyName = property.Name;
            foreach (VaultSecretNameAttribute attribute in property.GetCustomAttributes(typeof(VaultSecretNameAttribute)))
            {
                keyName = attribute.Name;
            }
            //append suffix
            foreach (VariousSuffixKeySet attribute in property.GetCustomAttributes(typeof(VariousSuffixKeySet)))
            {
                var suffix = "1";//default to 1
                try
                {
                    var configValue = ConfigHelper.GetString(attribute.SuffixKey);
                    if (!string.IsNullOrEmpty(configValue))
                        suffix = configValue;
                }
                catch { }
                keyName += suffix;
            }
            return keyName;
        }

        #endregion

        #region Azure Auth Get Token

        public virtual ClientCredential GetClientCredential()
        {
            string clientId = ConfigHelper.GetString("KeyVault.AuthClientId");
            string secret = ConfigHelper.GetString("KeyVault.AuthClientSecret");

            return new ClientCredential(clientId, secret);

        }

        protected async Task<string> GetToken(string authority, string resource, string scope)
        {
            ClientCredential clientCred = GetClientCredential();
            var authContext = new AuthenticationContext(authority);
            AuthenticationResult result = await authContext.AcquireTokenAsync(resource, clientCred);

            if (result == null)
                throw new InvalidOperationException("Failed to obtain the JWT token");

            return result.AccessToken;
        }

        #endregion
    }
    internal class CoreVaultBase : VaultBase
    {
        public CoreVaultBase()
        {
            VaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(GetToken));
            try
            {
                AzureVaultAddress = $"https://{ConfigHelper.GetString("KeyVault.Core.VaultName")}.vault.azure.net/";
            }
            catch
            {
            }
        }
        public override ClientCredential GetClientCredential()
        {
            string clientId = ConfigHelper.GetString("KeyVault.Core.AuthClientId");
            string secret = ConfigHelper.GetString("KeyVault.Core.AuthClientSecret");
            return new ClientCredential(clientId, secret);
        }
    }
}