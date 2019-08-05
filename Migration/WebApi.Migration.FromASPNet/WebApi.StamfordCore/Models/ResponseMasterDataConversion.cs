using System.Collections.Generic;

namespace WebApi.StamfordCore.Models
{
    public class ResponseMasterDataConversion
    {
        public List<string> VendorBlockNumberSuccess { get; set; }

        public List<string> VendorBlockNumberFail { get; set; }

        public List<string> VendorErrorMessage { get; set; }

        public List<string> VendorTransactionSuccessList { get; set; }

        public List<string> AddressBlockNumberSuccess { get; set; }

        public List<string> AddressBlockNumberFail { get; set; }

        public List<string> AddressErrorMessage { get; set; }
        public List<string> AddressTransactionSuccessList { get; set; }

        public List<string> ListValidate { get; set; }
        public ResponseMasterDataConversion()
        {
            VendorBlockNumberSuccess = new List<string>();
            VendorBlockNumberFail = new List<string>();
            VendorErrorMessage = new List<string>();
            VendorTransactionSuccessList = new List<string>();
            AddressBlockNumberSuccess = new List<string>();
            AddressBlockNumberFail = new List<string>();
            AddressErrorMessage = new List<string>();
            AddressTransactionSuccessList = new List<string>();
            ListValidate = new List<string>();
        }
    }
}