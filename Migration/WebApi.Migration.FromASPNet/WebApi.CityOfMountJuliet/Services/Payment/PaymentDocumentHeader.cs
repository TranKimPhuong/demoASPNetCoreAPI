using WebApi.CityOfMountJuliet.Models.Data.Provider;
using WebApi.CityOfMountJuliet.Models.Data.Provider.FormatAttributes;
using WebApi.CommonCore.Helper;

namespace WebApi.CityOfMountJuliet.Services.Payment
{
    //TODO: define additional field as public properties 
    internal class PaymentDocumentHeader : DocumentHeader
    {
        public string PaymentNumber { get; set; } = string.Empty;
        [FormatDate(outputFormat: "MM/dd/yyyy", inputFormats: new[] { "M/d/y", "M-d-y" })]
        public string PaymentDate { get; set; } = string.Empty;
        [FormatAmount]
        public string PaymentAmount { get; set; } = string.Empty;
        public string PayeeId { get; set; } = string.Empty;

        public string FreeFormAddress1 { get; set; } = string.Empty;
        public string FreeFormAddress2 { get; set; } = string.Empty;
        public string FreeFormAddress3 { get; set; } = string.Empty;
        public string FreeFormAddress4 { get; set; } = string.Empty;
        public string FreeFormAddress5 { get; set; } = string.Empty;
        public string FreeFormAddress6 { get; set; } = string.Empty;
        public override string ToString()
        {
            return $"H,{PaymentNumber.Trim()},{PaymentAmount.Trim()},{PayeeId.Trim()},{PaymentDate.Trim()},{FreeFormAddress1?.Trim().EscapeCSV()},{FreeFormAddress2?.Trim().EscapeCSV()},{FreeFormAddress3?.Trim().EscapeCSV()},{FreeFormAddress4?.Trim().EscapeCSV()},{FreeFormAddress5?.Trim().EscapeCSV()}";
        }
    }
}