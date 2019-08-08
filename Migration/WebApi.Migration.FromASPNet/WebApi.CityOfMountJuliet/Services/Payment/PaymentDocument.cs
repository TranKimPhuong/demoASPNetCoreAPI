using System;
using System.Collections.Generic;
using System.Linq;
using WebApi.CityOfMountJuliet.Models.Data.Maps;
using WebApi.CityOfMountJuliet.Models.Data.Provider;
using WebApi.CityOfMountJuliet.Models.Library;
using WebApi.CommonCore.Helper;

namespace WebApi.CityOfMountJuliet.Services.Payment
{
    internal class PaymentDocument : Document
    {
        internal PaymentDocument(DocumentHeader header) : base(header)
        {
        }
        public override string ToString()
        {
            return $"{ Header.ToString()}{Environment.NewLine}"
                + $"{string.Join(Environment.NewLine, DetailLines.Select(d => d.ToString()))}";
        }

        internal override DocumentDetailLine CreateNewDocumentDetailLine()
        {
            return new PaymentDocumentDetailLine();
        }

        internal override void AssignHeader(Map map, Page page, Dictionary<string, List<string>> addressPreFixs)
        {
            base.AssignHeader(map, page, addressPreFixs);
        }

        internal override void AssignDetail(Map map, List<Page> pageDetails, List<Page> pageRemittances)
        {
            {
                var mapDetail = map.MapDetail;
                var mapRemittance = map.MapRemittance;
                if (mapDetail == null)
                    return;
                foreach (var pageDetail in pageDetails)
                {
                    string Description = "";
                    var fromRow = mapDetail.FromLine - 1;
                    var toRow = mapDetail.ToLine;
                    if (toRow == 0) toRow = pageDetail.Rows.Count;
                    var rows = pageDetail.Rows;

                    for (var rowIndex = fromRow; rowIndex < toRow; rowIndex++)
                    {
                        // omit Empty/Blank Lines
                        if (string.IsNullOrEmpty(rows[rowIndex].Trim()))
                            continue;

                        if (mapDetail.BreakDetailLineCondition.Count > 0
                            && PsTool.CheckCondition(mapDetail.BreakDetailLineCondition, rows[rowIndex]))
                            break;

                        if (mapDetail.RecognizeDetailLineCondition.Count > 0
                            && PsTool.CheckCondition(mapDetail.RecognizeDetailLineCondition, rows[rowIndex]))
                        {
                            AssignDetailLine(map, mapDetail, rows[rowIndex]);
                        }
                        if (mapDetail.RecognizeDetailLineCondition.Count == 0)
                        {
                            string dataLine = rows[rowIndex];
                            string InvDate = dataLine.TMid(1, 10);
                            if (!string.IsNullOrEmpty(InvDate))
                            {
                                DateTime? oDate = InvDate.ToDate("MM/dd/yyyy","M/dd/yyyy","M/d/yyyy");
                                if (!oDate.HasValue) {
                                    Description = dataLine.IndexOf(':') > 0 ? dataLine.Mid(dataLine.IndexOf(':') + 2).Trim() : ""; continue;
                                }
                                if (!string.IsNullOrEmpty(Description)) { dataLine = dataLine.TrimEnd() + $"   {Description}"; Description = "";}
                            }
                            AssignDetailLine(map, mapDetail, dataLine, "");
                        }
                    }
                }
                if (pageDetails.Count > 0
                  && mapRemittance != null && pageRemittances != null && pageRemittances.Count > 0)
                    AssignDetailFromRemitFile(map, pageDetails[0], pageRemittances);
            }
        }

        protected override void AssignDetailLine(Map map, MapDetail mapDetail, string dataLine, string columnHeaderName = "")
        {
            base.AssignDetailLine(map, mapDetail, dataLine, columnHeaderName);
        }

        protected override void AssignDetailFromRemitFile(Map map, Page sourcePage, List<Page> pageRemittances)
        {
            base.AssignDetailFromRemitFile(map, sourcePage, pageRemittances);
        }

        protected override void AssignDetailLineFromRemitFile(Map map, string dataLine, string columnHeaderName = "")
        {
            base.AssignDetailLineFromRemitFile(map, dataLine, columnHeaderName);
        }
    }
}