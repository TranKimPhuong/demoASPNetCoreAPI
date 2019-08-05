namespace WebApi.StamfordCore.Models
{
    public class VendorAddress
    {
        public string AddressName { set; get; }
        public string Address1 { set; get; }
        public string Address2 { set; get; }
        public string Address3 { set; get; }
        public string City { set; get; }
        public string StateProvince { set; get; }
        public string PostalCode { set; get; }
        public string Contact { set; get; }
        public string PhoneNumber { set; get; }
    
        public string PaymentMethod { set; get; }
      
        public VendorAddress()
        {
            this.AddressName = "MAIN";
            this.PaymentMethod = "0";
        }
        public override string ToString()
        {
            return @"""" + AddressName
                + @""",""" + Address1
                + @""",""" + Address2
                + @""",""" + Address3
                + @""",""" + City
                + @""",""" + StateProvince
                + @""",""" + PostalCode
                + @""",""" + Contact
                + @""",""" + PhoneNumber
                + @"""";

        }

    }
    public class VendorInfomation
    {
        public string VendorName { get; set; }
        public byte Status { get; } = 1;
        public string DefaultRemitToId { get; } = "MAIN";
        public string MainAddressId { get; } = "MAIN";
        public bool HoldStatus { get; } = false;
        public string AddressName { get; } = "MAIN";
        public string VendorKey { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Address3 { get; set; }
        public string City { get; set; }
        public string StateProvince { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; } = string.Empty;
    }
}