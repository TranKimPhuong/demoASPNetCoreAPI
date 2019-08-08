using System;
using WebApi.CityOfMountJuliet.Models.Data.Maps;

namespace WebApi.CityOfMountJuliet.Services.MasterData
{
    internal class MasterDataMap : Map
    {
        internal override void SetMapHeader()
        {
            DataFromLine = 2;
            AddressPreFixs.Add("VendorAddress");
            MapHeader.AddField("VendorId", 1);
            MapHeader.AddField("VendorName", 2);
            MapHeader.AddField("VendorAddress1", 3);
            MapHeader.AddField("VendorAddress2", 4);
            MapHeader.AddField("VendorAddress3", 5);
            MapHeader.AddField("VendorCity", 6);
            MapHeader.AddField("VendorState", 7);
            MapHeader.AddField("VendorZip", 8);
            MapHeader.AddField("VendorPhone", 9);
        }

        internal override void SetMapDetail()
        {
            return;
        }

        internal override void SetMapRemittance()
        {
            base.SetMapRemittance();
        }
    }
}