using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.CityOfMountJuliet.Models.Library
{
    public class InputFileProperties
    {
        public IFormFile File { get; set; }
        public string CaseAction { get; set; }
        //public string VendorCreate { get; set; }
        //public string AddressCreate { get; set; }
        //public string VendorUpdate { get; set; }
        //public string AddressUpdate { get; set; }
        public string ContainerName { get; set; }
        public string BlobName { get; set; }
        public string Decrypt { get; set; }
        public string SiteName { get; set; }
        public string isGetFromBlob { get; set; }
    }
}
