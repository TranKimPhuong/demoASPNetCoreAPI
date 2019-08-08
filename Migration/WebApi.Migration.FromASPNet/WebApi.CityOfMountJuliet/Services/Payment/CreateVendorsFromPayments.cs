using System;
using System.Collections.Generic;
using System.Linq;
using WebApi.CommonCore.Helper;
using WebApi.CityOfMountJuliet.Models.Library;
using WebApi.CityOfMountJuliet.Models.Data;

namespace WebApi.CityOfMountJuliet.Services.Payment
{
    internal class CreateVendorsFromPayments
    {
        List<PaymentDocumentHeader> _paymentDatas;
        public CreateVendorsFromPayments(IEnumerable<PaymentDocumentHeader> source)
        {
            this._paymentDatas = source.ToList();
        }
        public bool ProcessCreateVendors(out string errors)
        {
            errors = string.Empty;
            var vendors = this._paymentDatas.Select(vendorSelector);
            var dt = vendors.ToDataTable();
            var dao = DbServiceFactory.GetCurrent();
            if (dao == null)
            {
                throw new Exception("Can not get customer data connection");
            }
            IDictionary<string, object> parameters = new Dictionary<string, object>();
            parameters["@PaymentConversionVendorAddressesTvp"] = dt;
            object oResult = dao.ProcForScalar("dbo.usp_i_CreateVendorFromPaymentFile", parameters);
            if (oResult != null && !AppHelper.IsNumeric(oResult))
            {
                errors = AppHelper.ToString(oResult);
                return false;
            }
            return true;
        }
        Func<PaymentDocumentHeader, VendorInfomation> vendorSelector = (h)
             =>
        {
            var vendor = new VendorInfomation();
            vendor.VendorKey = h.PayeeId;
            var freeFormAddresses = new[]
            {
                 h.FreeFormAddress1,
                 h.FreeFormAddress2,
                 h.FreeFormAddress3,
                 h.FreeFormAddress4,
                 h.FreeFormAddress5,
                 h.FreeFormAddress6
             }
            .Where(s => !string.IsNullOrWhiteSpace(s) && s.Trim() != ",").ToList();

            string city = string.Empty, state = string.Empty, zip = string.Empty;
            if (freeFormAddresses.Count > 0)
            {
                vendor.VendorName = freeFormAddresses[0];
                freeFormAddresses.Remove(freeFormAddresses.First());

            };
            if (freeFormAddresses.Count > 0)
            {
                var CountryCode = CountryCodeConverter.ConvertToThreeLetterISORegionName(freeFormAddresses.Last());
                if (!string.IsNullOrEmpty(CountryCode))
                {
                    vendor.Country = CountryCode;
                    freeFormAddresses.Remove(freeFormAddresses.Last());
                }
                
                if (freeFormAddresses.Last().TryParseCityStateZip(out city, out state, out zip))
                {
                    vendor.City = city;
                    vendor.StateProvince = state;
                    vendor.PostalCode = zip;
                    freeFormAddresses.Remove(freeFormAddresses.Last());
                    if (freeFormAddresses.Count > 0 && !string.IsNullOrEmpty(zip) && string.IsNullOrEmpty(city))
                    {
                        freeFormAddresses.Last().TryParseCityStateZip(out city, out state, out zip);
                        vendor.City = city;
                        if (string.IsNullOrEmpty(vendor.StateProvince)) vendor.StateProvince = state;
                        freeFormAddresses.Remove(freeFormAddresses.Last());                    
                    }
                }
            }
            for (int i = 0; i < freeFormAddresses.Count; i++)
            {
                var property = vendor.GetType().GetProperties().FirstOrDefault(p => p.Name == $"Address{i + 1}");
                if (property != null)
                {
                    property.SetValue(vendor, freeFormAddresses[i]);
                }
            }
            return vendor;
        };
        public class VendorInfomation
        {
            public string VendorName { get; set; }
            public Byte Status { get; } = 1;
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
            public string Country { get; set; }
        }
    }
}