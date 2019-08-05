using System;

namespace WebApi.CommonCore.KeyVault.Attributes
{
    internal class SuffixKeySet
    {
        public const string Sql = "SuffixKeySet.Sql";
        public const string Storage = "SuffixKeySet.Storage";
        public const string ServiceBus = "SuffixKeySet.ServiceBus";
    }
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    internal class VariousSuffixKeySet : Attribute
    {
        private string _suffixKey;
        public VariousSuffixKeySet(string suffixKey) { _suffixKey = suffixKey; }
        public string SuffixKey => _suffixKey;
    }
}
