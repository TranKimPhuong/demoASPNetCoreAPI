using Newtonsoft.Json;
using System;

namespace WebApi.CommonCore.Models
{
    public class ConversionRequest
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public String blobHeaderName { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public String blobDetailName { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public String blobOutputName { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public String containerName { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public String prefixPayeeID { get; set; }
    }
}
