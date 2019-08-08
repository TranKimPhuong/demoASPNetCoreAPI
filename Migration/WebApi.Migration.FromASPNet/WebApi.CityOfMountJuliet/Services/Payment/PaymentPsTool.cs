using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WebApi.CityOfMountJuliet.Models.Data.Maps;
using WebApi.CityOfMountJuliet.Models.Data.Provider;
using WebApi.CityOfMountJuliet.Models.Library;

namespace WebApi.CityOfMountJuliet.Services.Payment
{
    internal class PaymentPsTool : PsTool
    {
        protected override Map CreateMap()
        {
            return new PaymentMap();
        }

        protected override Document CreateNewDocument()
        {
            return new PaymentDocument(new PaymentDocumentHeader());
        }

        protected override void ProcessBeforeConvertDocumentsToStandardFile()
        {
            #region Createvendorfrompayment
            var error = string.Empty;
            var module = new CreateVendorsFromPayments(this.Documents.Select(s => s.Header as PaymentDocumentHeader));
            if (!module.ProcessCreateVendors(out error))
            {
                throw new Exception(error);
            }
            #endregion

            #region Omit leading zero
            foreach (var doc in Documents)
            {
                var header = doc.Header as PaymentDocumentHeader;
                header.PaymentNumber = header.PaymentNumber.TrimStart('0');
            }
            #endregion
        }

        protected override StringBuilder BuildStandardFileHeader()
        {
            var res = new StringBuilder();
            res.AppendLine("H,PaymentNumber,PaymentAmount,PayeeId,PaymentDate,FreeFormAddressLine1,FreeFormAddressLine2,FreeFormAddressLine3,FreeFormAddressLine4,FreeFormAddressLine5");
            res.AppendLine("D,InvoiceNumber,InvoiceDate,NetAmount,UDF2,UDF1");
            return res;
        }
        protected override StringBuilder ConvertDocumentsToStandardFile()
        {
            var res = new StringBuilder();
            try
            {
                res.Append(string.Join(Environment.NewLine, Documents.Select(doc => doc.ToString())));
                return res;
            }
            catch (Exception ex)
            {
                ListErrors.Add(ex.Message);
            }
            return res;
        }

        protected override List<Page> FillData(byte[] inputFile)
        {
            var pages = new List<Page>();
            var page = new Page
            {
                FromRow = Map.DataFromLine
            };
            var i = 1;
            try
            {
                using (var stream = new MemoryStream(inputFile))
                using (var reader = new StreamReader(stream))
                {
                    while (!reader.EndOfStream)
                    {
                        var dataLine = reader.ReadLine();
                        if (Map.NewPageCondition.Count > 0
                            && CheckCondition(Map.NewPageCondition, dataLine))
                        {
                            // truong hop ko co ky tu ket thuc page thi dong 1 page cu truoc khi bat dau 1 page moi
                            if (page != null && page.Rows.Count > 0)
                            {
                                page.ToRow = i - 1;
                                SetKeyAndOverflow(page);
                                pages.Add(page);
                            }

                            page = new Page
                            {
                                FromRow = i
                            };
                        }

                        if (page == null || i < page.FromRow)
                        {
                            i++;
                            continue;
                        }

                        page.Rows.Add(dataLine);
                        if ((Map.BreakPageCondition.Count == 0 && Map.BreakPageLine == 0 && dataLine == "\f")
                            ||
                            (Map.BreakPageCondition.Count > 0 &&
                             CheckCondition(Map.BreakPageCondition, dataLine))
                            || i == page.FromRow + Map.BreakPageLine - 1)
                        {
                            page.ToRow = i;
                            SetKeyAndOverflow(page);

                            pages.Add(page);
                            page = null;

                            // khong kiem tra ky tu bat dau 1 page moi thi tao new page moi luon
                            if (Map.NewPageCondition.Count == 0)
                            {
                                page = new Page
                                {
                                    FromRow = i + 1
                                };
                            }
                        }
                        i++;
                    }
                }
            }
            catch (Exception ex)
            {
                if (string.IsNullOrEmpty(Map.MapHeader.FieldKey))
                {
                    if (page != null)
                        ListErrors.Add(
                            $"Read file error: Line {page.FromRow} - {page.ToRow}, rows - {page.Rows.Count} : {ex.Message}{Environment.NewLine}");
                }
                else if (page != null)
                    ListErrors.Add(
                        $"Read file error: Line {page.FromRow} - {page.ToRow}, {Map.MapHeader.FieldKey} - {page.Key}, Rows - {page.Rows.Count} : {ex.Message}{Environment.NewLine}");
            }

            if (page != null && page.Rows.Count > 0)
            {
                page.ToRow = i - 1;
                SetKeyAndOverflow(page);
                pages.Add(page);
            }

            //Omit Blank Page
            pages = pages.Where(x => x.Rows.Where(y => !string.IsNullOrWhiteSpace(y)).Any()).ToList();

            #region for last page don't match with above condition to break page

            if (page != null && page.Rows.Count > 0 && page.Rows.Count < Map.BreakPageLine)
            {
                // insert more line up to BreakPageLine
                for (var index = page.Rows.Count; index < Map.BreakPageLine; index++)
                {
                    page.Rows.Add("");
                }

                // check overflow 
                if (Map.MapOverflow != null && Map.MapOverflow.Line > 0 &&
                    !string.IsNullOrEmpty(Map.MapOverflow.Key))
                {
                    if (page.Rows.Count >= Map.MapOverflow.Line
                        && !string.IsNullOrEmpty(page.Rows[Map.MapOverflow.Line - 1]))
                    {
                        page.HasOver = page.Rows[Map.MapOverflow.Line - 1].Trim().Contains(Map.MapOverflow.Key.Trim());
                    }
                }

                // set page key
                var mapkey = Map.MapHeader.MapFields.FirstOrDefault(e => e.FieldName == Map.MapHeader.FieldKey);
                if (mapkey != null && page.Rows.Count >= mapkey.Line)
                {
                    if (!string.IsNullOrEmpty(Map.Delimiter))
                    {
                        var rowItems =
                            page.Rows[mapkey.Line - 1].Split(new[] { Map.Delimiter }, StringSplitOptions.None).
                                ToList();
                        if (rowItems.Count >= mapkey.Start)
                            page.Key = rowItems[mapkey.Start - 1];
                    }
                    else page.Key = page.Rows[mapkey.Line - 1].TMid(mapkey.Start, mapkey.Length);
                }
            }

            #endregion

            if (!string.IsNullOrEmpty(Map.MapHeader.FieldKey))
            {
                if (pages.Any(e => (string.IsNullOrEmpty(e.Key) && !e.HasOver)))
                    throw new Exception("The input file contains INVALID DATA");
                pages = pages.Where(e => (!string.IsNullOrEmpty(e.Key) && !e.HasOver) || e.HasOver).ToList();

            }
            if (pages.Count == 0)
                ListErrors.Add("The input file contains INVALID DATA");
            else
            {
                if (Map.ValidatePosition.Count > 0)
                    if (!ValidateFileFormatBySpecificPos(pages[0], Map.ValidatePosition, ""))
                        ListErrors.Add("The input file is INCORRECT format.\r\n");
            }
            return pages;
        }

        protected override void Process()
        {
            base.Process();
        }
    }
}