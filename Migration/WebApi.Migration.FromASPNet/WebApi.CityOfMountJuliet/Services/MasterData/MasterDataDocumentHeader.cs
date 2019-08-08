using WebApi.CityOfMountJuliet.Models.Data.Provider;

namespace WebApi.CityOfMountJuliet.Services.MasterData
{
    //TODO: define additional field as public properties 
    internal class MasterDataDocumentHeader : DocumentHeader
    {
        public string VendorId { get; set; } = string.Empty;
        public string VendorName { get; set; } = string.Empty;
        public string VendorAddress1 { get; set; } = string.Empty;
        public string VendorAddress2 { get; set; } = string.Empty;
        public string VendorAddress3 { get; set; } = string.Empty;
        public string VendorCity { get; set; } = string.Empty;
        public string VendorState { get; set; } = string.Empty;
        public string VendorZip { get; set; } = string.Empty;
        public string VendorPhone { get; set; } = string.Empty;
    }
}