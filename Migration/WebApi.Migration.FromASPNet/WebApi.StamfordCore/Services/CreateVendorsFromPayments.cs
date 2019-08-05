using System;
using System.Collections.Generic;
using System.Linq;
using WebApi.CommonCore.Helper;
using WebApi.StamfordCore.Models;
using WebApi.StamfordCore.Models.Data;
using WebApi.StamfordCore.Services;

namespace WebApi.StamfordCore.Services.Payment
{
    internal class CreateVendorsFromPayments
    {
        List<PaymentConversionLine> _paymentDatas;
        public CreateVendorsFromPayments(IEnumerable<PaymentConversionLine> source)
        {
            this._paymentDatas = source.ToList();
        }
        public bool ProcessCreateVendors(out string errors)
        {
            errors = string.Empty;
            var vendors = _paymentDatas.Select(vendorSelector);
            var dt = vendors.AsDataTable();
            var dao = DbServiceFactory.GetCurrent();
            if (dao == null)
            {
                throw new Exception("Can not get customer data connection");
            }
            IDictionary<string, object> parameters = new Dictionary<string, object>();
            parameters["@PaymentConversionVendorAddressesTvp"] = dt;

            object oResult = dao.ProcForScalar("dbo.usp_i_CreateVendorFromPaymentFile", parameters);
            if (oResult != null && !AppHelper.IsNumeric(oResult)) //error
            {
                errors = AppHelper.ToString(oResult);
                return false;
            }
            return true;
        }
        Func<PaymentConversionLine, VendorInfomation> vendorSelector = (h)
             =>
         {

             var vendor = new VendorInfomation()
             {
                 VendorKey = h.PayeeID,
                 VendorName = h.PayeeName,
                 City = h.City,
                 StateProvince = h.StateProvince,
                 PostalCode = h.PostalCode,
             };
             var addresses = new[] { h.Address1, h.Address2, h.Address3 }
                            .Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
             for (int i = 0; i < addresses.Count; i++)
             {
                 var property = vendor.GetType().GetProperties().FirstOrDefault(p => p.Name == $"Address{i + 1}");
                 if (property != null)
                 {
                     property.SetValue(vendor, addresses[i]);
                 }
             }
             return vendor;
         };
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
}