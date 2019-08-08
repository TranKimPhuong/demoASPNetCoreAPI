using System.Collections.Generic;
using System.Linq;
using WebApi.CityOfMountJuliet.Models.Data.Maps;
using WebApi.CityOfMountJuliet.Models.Library;

namespace WebApi.CityOfMountJuliet.Models.Data.Provider
{
    internal abstract class Document
    {
        //TODO: what document header type u want to use
        protected Document(DocumentHeader header)
        {
            Header = header;
            DetailLines = new List<DocumentDetailLine>();
        }

        internal DocumentHeader Header { get; }
        internal List<DocumentDetailLine> DetailLines { get; }
        internal abstract DocumentDetailLine CreateNewDocumentDetailLine();

        internal virtual void AssignHeader(Map map, Page page, Dictionary<string, List<string>> addressPreFixs)
        {
            if (!string.IsNullOrEmpty(map.Delimiter))
            {
                var row = page.Rows.FirstOrDefault();
                if (string.IsNullOrEmpty(row)) return;
                var fieldDatas = row.LoadListFields(map.Delimiter, "");
                foreach (var field in map.MapHeader.MapFields)
                {
                    var value = field.ElementIndex - 1 < fieldDatas.Count
                        ? fieldDatas[field.ElementIndex - 1]
                        : string.Empty;
                    Header.SetPropertyValue(field.FieldName, value);
                }
            }
            else
            {
                foreach (var field in map.MapHeader.MapFields)
                {
                    if (field.Line > page.Rows.Count) continue;

                    var row = string.IsNullOrEmpty(field.StartWith)
                        ? page.Rows[field.Line - 1]
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
        }

        internal virtual void AssignDetail(Map map, List<Page> pageDetails, List<Page> pageRemittances)
        {
            var mapDetail = map.MapDetail;
            var mapRemittance = map.MapRemittance;
            if (mapDetail == null)
                return;
            foreach (var pageDetail in pageDetails)
            {
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
                        // for one line one payment with out no condition to recongnize any detail
                        AssignDetailLine(map, mapDetail, rows[rowIndex], "");
                    }
                }
            }
            if (pageDetails.Count > 0
              && mapRemittance != null && pageRemittances != null && pageRemittances.Count > 0)
                AssignDetailFromRemitFile(map, pageDetails[0], pageRemittances);
        }

        protected virtual void AssignDetailFromRemitFile(Map map, Page sourcePage, List<Page> pageRemittances)
        {
            var mapRemittance = map.MapRemittance;
            // Get information of link field between Check file and Remit file
            var linkFieldInfo = map.MapHeader.MapFields.FirstOrDefault(e => e.FieldName == mapRemittance.LinkFieldName);
            var keyFieldValue = sourcePage.Rows[linkFieldInfo.Line - 1].TMid(linkFieldInfo.Start, linkFieldInfo.Length);

            // Get pages of remit(depend) file that exist in check(master) file
            var remitPages = pageRemittances.Where(r => r.Key == keyFieldValue).ToList();
            if (!remitPages.Any())
            {
                return;
            }
            foreach (var remitPage in remitPages)
            {
                int fromRow = mapRemittance.DetailFromLine - 1;
                var toRow = mapRemittance.DetailToLine;
                if (toRow == 0) toRow = remitPage.Rows.Count;

                var rows = remitPage.Rows;

                for (int rowIndex = fromRow; rowIndex < toRow; rowIndex++)
                {
                    // omit Empty/Blank Lines
                    if (string.IsNullOrEmpty(rows[rowIndex].Trim())) continue;

                    // Detail main and comment has same codition
                    if (mapRemittance.BreakDetailLineCondition.Count > 0
                        && PsTool.CheckCondition(mapRemittance.BreakDetailLineCondition, rows[rowIndex]))
                        break;

                    // Detail main/ Detail Comment
                    if (mapRemittance.RecognizeDetailLineCondition.Count > 0
                        && PsTool.CheckCondition(mapRemittance.RecognizeDetailLineCondition, rows[rowIndex]))
                    {
                        AssignDetailLineFromRemitFile(map, rows[rowIndex]);
                    }
                    if (mapRemittance.RecognizeDetailLineCondition.Count == 0)
                    {
                        AssignDetailLineFromRemitFile(map, rows[rowIndex]);
                    }
                }
            }
        }

        protected virtual void AssignDetailLine(Map map, MapDetail mapDetail, string dataLine, string ColumnHeaderName = "")
        {
            var firstField = mapDetail.MapFields.FirstOrDefault();
            var startFirstField = firstField?.Start ?? 0;

            for (int j = 0; j < mapDetail.DetailCount; j++)
            {
                // "start" is used when detail line is on 1 line
                var start = startFirstField + j * mapDetail.DetailLength;

                // chi insert khi doan data cua Detail khac rong
                if (!string.IsNullOrWhiteSpace(dataLine.TMid(start, mapDetail.DetailLength)))
                {
                    var docDetailLine = CreateNewDocumentDetailLine();
                    // Go through each field to assign data
                    foreach (var field in mapDetail.MapFields)
                    {
                        var value = string.Empty;
                        if (!string.IsNullOrEmpty(map.Delimiter))
                        {
                            var rowItems = dataLine.LoadListFields(map.Delimiter, "").ToList();
                            var pos = -1;

                            if (map.ColumnNameRow == 0)
                                pos = field.Start - 1;
                            else
                                pos = GetPositionOfField(map, ColumnHeaderName, field.ColumnName);

                            if (rowItems.Count >= pos && pos != -1) value = rowItems[pos];
                        }
                        else value = dataLine.Mid(field.Start + j * mapDetail.DetailLength, field.Length);

                        docDetailLine.SetPropertyValue(field.FieldName, value);
                    }
                    // Add detail 
                    DetailLines.Add(docDetailLine);
                }
            }
        }

        protected virtual void AssignDetailLineFromRemitFile(Map map, string dataLine, string columnHeaderName = "")
        {
            var mapRemittance = map.MapRemittance;
            var firstField = mapRemittance.MapFields.FirstOrDefault();
            var startFirstField = firstField?.Start ?? 0;

            for (int j = 0; j < mapRemittance.DetailCount; j++)
            {
                // "start" is used when detail line is on 1 line
                var start = startFirstField + j * mapRemittance.DetailLength;

                // chi insert khi doan data cua Detail khac rong
                if (!string.IsNullOrWhiteSpace(dataLine.TMid(start, mapRemittance.DetailLength)))
                {
                    var docDetailLine = CreateNewDocumentDetailLine();
                    // Go through each field to assign data
                    foreach (var field in mapRemittance.MapFields)
                    {
                        var value = string.Empty;
                        if (!string.IsNullOrEmpty(map.Delimiter))
                        {
                            var rowItems = dataLine.LoadListFields(map.Delimiter, "").ToList();
                            var pos = -1;

                            if (map.ColumnNameRow == 0)
                                pos = field.Start - 1;
                            else
                                pos = GetPositionOfField(map, columnHeaderName, field.ColumnName);

                            if (rowItems.Count >= pos && pos != -1) value = rowItems[pos];
                        }
                        else value = dataLine.Mid(field.Start + j * mapRemittance.DetailLength, field.Length);

                        docDetailLine.SetPropertyValue(field.FieldName, value);
                    }
                    // Add detail 
                    DetailLines.Add(docDetailLine);
                }
            }
        }

        protected int GetPositionOfField(Map map, string columnHeaderName, string sFieldName)
        {
            if (columnHeaderName.Contains(sFieldName.ToUpper()))
            {
                var fieldNames = columnHeaderName.LoadListFields(map.Delimiter, "").ToList();
                var pos = fieldNames.FindIndex(e => e.Trim().ToUpper() == sFieldName.ToUpper());
                return pos;
            }

            return -1;
        }
    }
}