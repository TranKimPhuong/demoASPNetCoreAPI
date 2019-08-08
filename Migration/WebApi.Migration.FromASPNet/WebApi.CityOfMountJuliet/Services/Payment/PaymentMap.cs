using System;
using WebApi.CityOfMountJuliet.Models.Data.Maps;

namespace WebApi.CityOfMountJuliet.Services.Payment
{
    internal class PaymentMap : Map
    {
        internal override void SetMapHeader()
        {
            MapOverflow = new MapOverflow("VOID VOID VOID VOID VOID VOID", 75, false);
            BreakPageLine = 81;
            AddressPreFixs.Add("FreeFormAddress");
            MapHeader.AddField("PaymentNumber", 73, 57, 8);
            MapHeader.AddField("PaymentDate", 73, 38, 10);
            MapHeader.AddField("PaymentAmount", 73, 66, 13);
            MapHeader.AddField("PayeeId", 81, 12, 8);
            MapHeader.AddField("FreeFormAddress1", 75, 12, 35);
            MapHeader.AddField("FreeFormAddress2", 76, 12, 50);
            MapHeader.AddField("FreeFormAddress3", 77, 12, 50);
            MapHeader.AddField("FreeFormAddress4", 78, 12, 50);
            MapHeader.AddField("FreeFormAddress5", 79, 12, 50);
        }
        internal override void SetMapDetail()
        {
            MapDetail.FromLine = 1;
            MapDetail.BreakDetailLineCondition.Add(new Condition("Cash Account:", 1, 1, 13));
            MapDetail.AddField("InvoiceDate", 1, 10);
            MapDetail.AddField("Description", 81, 35);
            MapDetail.AddField("InvoiceNumber", 40, 12);
            MapDetail.AddField("PONumber", 53, 11);
            MapDetail.AddField("NetAmount", 65, 14); //get positive value
        }

        internal override void SetMapRemittance()
        {
            base.SetMapRemittance();
        }
    }
}