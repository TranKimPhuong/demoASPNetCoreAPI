using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.CommonCore.Models.Vendor
{
    [Obsolete("Remove after key vault migration complete for all project.")]
    public class VendorAddress
    {

        public string VendorName { get; set; }
        public Byte Status { get; set; }
        public string DefaultRemitToId { get; set; }
        public string MainAddressId { get; set; }
        public Boolean HoldStatus { get; set; }
        public Byte VirtualCardStatus { get; set; }
        public string AddressName { get; set; }
        public string VendorKey { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Address3 { get; set; }
        public string City { get; set; }
        public string StateProvince { get; set; }
        public string PostalCode { get; set; }
        public Decimal PaymentMethod { get; set; }

        public VendorAddress()
        {
            this.MainAddressId = "MAIN";
            this.DefaultRemitToId = "MAIN";
            this.Status = 1;
            this.HoldStatus = false;
            this.VirtualCardStatus = 1;
            this.AddressName = "MAIN";
            this.PaymentMethod = new Decimal(0);
        }

        public VendorAddress(string VendorKey) : this()
        {
            this.VendorKey = VendorKey;
        }
    }
}