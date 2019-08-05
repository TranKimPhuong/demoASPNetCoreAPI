using System;

namespace WebApi.CommonCore.KeyVault.Attributes
{
    /// <summary>
    /// Map the property with a specify SecretName
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    internal sealed class VaultSecretNameAttribute : Attribute
    {
        readonly string _name;
        /// <param name="Name">SecretName</param>
        public VaultSecretNameAttribute(string Name)
        {
            this._name = Name;
        }

        public string Name => _name;
    }

}
