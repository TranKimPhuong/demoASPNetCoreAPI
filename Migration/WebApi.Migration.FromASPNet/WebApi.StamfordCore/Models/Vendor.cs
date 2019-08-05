namespace WebApi.StamfordCore.Models
{
    public class Vendor
    {
        public string VendorId { set; get; }
        public string Name { set; get; }
        public string Status { set; get; }

        public string Is1099 { set; get; }

        public string DefaultRemitToId { set; get; }

        public string MainAddressId { set; get; }

        public string HoldStatus { set; get; }

        public Vendor()
        {
            this.Status = "1";
            this.Is1099 = "0";
            this.DefaultRemitToId = "MAIN";
            this.MainAddressId = "MAIN";
            this.HoldStatus = "0";
        }

        public override string ToString()
        {
            return @"""" + VendorId
                + @""",""" + Name
                + @""",""" + Status
                + @""",""" + Is1099
                + @""",""" + DefaultRemitToId
                + @""",""" + MainAddressId
                + @""",""" + HoldStatus + @"""";
        }

    }
}