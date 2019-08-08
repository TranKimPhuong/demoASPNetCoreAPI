using System.Collections.Generic;
using System.Linq;
using WebApi.CityOfMountJuliet.Models.Data.Maps;
using WebApi.CityOfMountJuliet.Models.Data.Provider;
using WebApi.CityOfMountJuliet.Models.Library;

namespace WebApi.CityOfMountJuliet.Services.MasterData
{
    internal class MasterDataDocument : Document
    {
        internal MasterDataDocument(DocumentHeader header) : base(header)
        {
        }

        internal override DocumentDetailLine CreateNewDocumentDetailLine()
        {
            return new MasterDataDocumentDetailLine();
        }

        internal override void AssignHeader(Map map, Page page, Dictionary<string, List<string>> addressPreFixs)
        {
            foreach (var field in map.MapHeader.MapFields)
            {
                if (field.Line > page.Rows.Count) continue;

                var row = string.IsNullOrEmpty(field.StartWith)
                    ? page.Rows[field.ElementIndex - 1]
                    : page.Rows.FirstOrDefault(e => e.StartsWith(field.StartWith));

                var value = row.TMid(field.Start, field.Length);
                // neu la address thi dua vao list xu ly sau
                if (map.AddressPreFixs.Count > 0 && map.AddressPreFixs.Any(address => field.FieldName.StartsWith(address)))
                {
                    var addressPreFix = map.AddressPreFixs.First(address => field.FieldName.StartsWith(address));
                    addressPreFixs[addressPreFix].Add(value);
                }
                else Header.SetPropertyValue(field.FieldName, value);
            }


            #region Assign Address
            foreach (var addressPreFix in map.AddressPreFixs)
            {
                addressPreFixs[addressPreFix] = addressPreFixs[addressPreFix].Where(e => !string.IsNullOrEmpty(e)).ToList();
                var mapAddress = map.MapHeader.MapFields
                    .Where(e => e.FieldName.StartsWith(addressPreFix))
                    .Select(e => e.FieldName)
                    .ToList();
                for (var i = 0; i < mapAddress.Count && i < addressPreFixs[addressPreFix].Count; i++)
                {
                    Header.SetPropertyValue(addressPreFix + (i + 1), addressPreFixs[addressPreFix][i]);
                }

                addressPreFixs[addressPreFix] = new List<string>();
            }
            #endregion
        }

        internal override void AssignDetail(Map map, List<Page> pageDetails, List<Page> pageRemittances)
        {
            return;
        }
        protected override void AssignDetailLine(Map map, MapDetail mapDetail, string dataLine, string columnHeaderName = "")
        {
            return;
        }
    }
}