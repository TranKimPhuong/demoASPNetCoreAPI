using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WebApi.CityOfMountJuliet.Models.Data.Maps;
using WebApi.CommonCore.Models;
using WebApi.CityOfMountJuliet.Models.Library;
using FormFile = System.Collections.Generic.KeyValuePair<string, byte[]>;

namespace WebApi.CityOfMountJuliet.Models.Data.Provider
{
    internal abstract class PsTool
    {
        protected ConversionRequest CurrentConversionRequest { get; private set; } = null;
        internal void SetCurrentConversionRequest(ConversionRequest request)
        {
            CurrentConversionRequest = request;
        }
        protected Dictionary<string, List<string>> AddressPreFixs;

        protected Map Map;
        protected List<Page> Pages;
        protected List<Page> PageRemittances;

        protected IList<Document> Documents = new List<Document>();
        protected List<string> ListErrors { get; } = new List<string>();

        internal List<string> GetErrors()
        {
            return ListErrors;
        }

        protected virtual void Initialize()
        {
            Map = CreateMap();
            // create map
            Map.Initialize();

            Pages = new List<Page>();
            AddressPreFixs = new Dictionary<string, List<string>>();
            foreach (var addressField in Map.AddressPreFixs)
            {
                AddressPreFixs[addressField] = new List<string>();
            }
        }

        private StringBuilder ProcessCore()
        {
            Process();
            if (ListErrors.Any())
                return new StringBuilder();

            BeforeConvertDataToStandardFile();
            if (ListErrors.Any())
                return new StringBuilder();

            var convertResult = ConvertDataToStandardFile();
            if (ListErrors.Any() || convertResult == null)
                return new StringBuilder();

            ProcessAfterProcessDataFile();
            if (ListErrors.Any())
                return new StringBuilder();

            var result = new StringBuilder();
            var headerContent = this.BuildStandardFileHeader();
            result.Append(headerContent.ToString());
            result.Append(convertResult);

            return result;
        }

        protected virtual void Process()
        {
            var pageDetails = new List<Page>();

            if (Map.MapHeader != null)
            {
                foreach (var page in Pages)
                {
                    pageDetails.Add(page);

                    if (page.HasOver)
                        continue;
                    try
                    {
                        // Nếu Page chứa Header ở đầu tiên, sau đó đến các page Overflow
                        // thì add cũ detail trước khi add 1 header mới
                        if (Map.MapOverflow != null && Map.MapOverflow.HeaderInFirstPage)
                        {
                            Document document;
                            if (Documents.Count == 0)
                            {
                                document = CreateNewDocument();
                                Documents.Add(document);
                                // Asign Header
                                document.AssignHeader(Map, page, AddressPreFixs);
                            }
                            else
                            {
                                pageDetails.Remove(page);
                                document = Documents.LastOrDefault();

                                // Asign Detail
                                document?.AssignDetail(Map, pageDetails, PageRemittances);

                                // Asign new Header
                                var newDocument = CreateNewDocument();
                                Documents.Add(newDocument);
                                newDocument.AssignHeader(Map, page, AddressPreFixs);

                                pageDetails = new List<Page> { page };
                            }
                        }
                        else
                        {
                            //Header in last page
                            var document = CreateNewDocument();
                            Documents.Add(document);

                            // Asign Header
                            document.AssignHeader(Map, page, AddressPreFixs);

                            // Asign Detail
                            document?.AssignDetail(Map, pageDetails, PageRemittances);
                            pageDetails = new List<Page>();
                        }

                    }
                    catch (Exception ex)
                    {
                        ListErrors.Add(
                            string.IsNullOrEmpty(Map.MapHeader.FieldKey)
                                ? $"From line {page.FromRow} to line {page.ToRow}: {ex.Message}{Environment.NewLine}"
                                : $"From line {page.FromRow} to line {page.ToRow}, [{Map.MapHeader.FieldKey}]={page.Key}: {ex.Message}{Environment.NewLine}");
                        if (ex is IncorrectFormatException)
                        {
                            break;
                        }
                    }
                }

                if (Map.MapOverflow == null || !Map.MapOverflow.HeaderInFirstPage || pageDetails.Count <= 0) return;
                {
                    var document = Documents.LastOrDefault();
                    // Asign Detail
                    document?.AssignDetail(Map, pageDetails, PageRemittances);
                }
            }
        }

        internal StringBuilder ProcessDataFile(byte[] primaryFile, byte[] remittanceFileContent = null)
        {
            Initialize();
            this.Pages = FillData(primaryFile);
            if (ListErrors.Any())
                return new StringBuilder();
            if (remittanceFileContent != null && Map.MapRemittance != null)
            {
                PageRemittances = FillRemittanceData(remittanceFileContent);
                if (ListErrors.Any())
                    return new StringBuilder();
            }
            return ProcessCore();
        }

        Func<string, string> fileErrorMsg = (s) => $"Errors in file \"{s}\"";

        internal StringBuilder ProcessMultiDataFile(IEnumerable<FormFile> primaryFiles, IEnumerable<FormFile> remittanceFiles)
        {
            Initialize();
            foreach (var item in primaryFiles)
            {
                var pages = FillData(item.Value);
                if (ListErrors.Any())
                {
                    ListErrors.Insert(0, fileErrorMsg(item.Key));
                    return new StringBuilder();
                }
                else
                {
                    Pages.AddRange(pages);
                }
            }
            if (remittanceFiles != null && remittanceFiles.Any() && Map.MapRemittance != null)
            {
                PageRemittances = new List<Page>();
                foreach (var item in remittanceFiles)
                {
                    var pages = FillRemittanceData(item.Value);
                    if (ListErrors.Any())
                    {
                        ListErrors.Insert(0, fileErrorMsg(item.Key));
                        return new StringBuilder();
                    }
                    else
                    {
                        PageRemittances.AddRange(pages);
                    }
                }
            }

            return ProcessCore();
        }

        protected virtual List<Page> FillData(byte[] inputFile)
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

        protected virtual List<Page> FillRemittanceData(byte[] remittanceFileContent)
        {
            var mapRemittance = this.Map.MapRemittance;
            var pages = new List<Page>();
            // When File exist
            int i = 1;
            Page page = new Page();
            page.FromRow = mapRemittance.DataFromLine;

            // Fill Data 
            using (var stream = new MemoryStream(remittanceFileContent))
            using (var streamReader = new StreamReader(stream))
            {
                while (!streamReader.EndOfStream)
                {
                    var dataLine = streamReader.ReadLine();

                    #region 1. dk nay dung khi gap dong dau tien cua page moi
                    if (mapRemittance.NewPageCondition.Count > 0 && CheckCondition(mapRemittance.NewPageCondition, dataLine))
                    {
                        // truong hop ko co ky tu ket thuc page thi dong 1 page cu truoc khi bat dau 1 page moi
                        if (page != null && page.Rows.Count > 0)
                        {
                            page.ToRow = i - 1;

                            this.SetKeyAndOverflowForRemittancePage(page);
                            pages.Add(page);
                        }

                        page = new Page();
                        page.FromRow = i;
                    }
                    #endregion

                    if (page == null || i < page.FromRow)
                    {
                        i++;
                        continue;
                    }

                    page.Rows.Add(dataLine);

                    #region 2. dk dung cho dong cuoi cung cua current page thoa dk nay
                    if ((mapRemittance.BreakPageCondition.Count == 0 && mapRemittance.BreakPageLine == 0 && dataLine == "\f")
                        || (mapRemittance.BreakPageCondition.Count > 0 && CheckCondition(mapRemittance.BreakPageCondition, dataLine))
                        || i == page.FromRow + mapRemittance.BreakPageLine - 1)
                    {
                        page.ToRow = i;
                        SetKeyAndOverflowForRemittancePage(page);

                        pages.Add(page);
                        page = null;

                        // khong kiem tra ky tu bat dau 1 page moi thi tao new page moi luon
                        if (this.Map.NewPageCondition.Count == 0)
                        {
                            page = new Page();
                            page.FromRow = i + 1;
                        }
                    }
                    #endregion

                    i++;
                }
            }

            #region for last page don't match with above condition to break page
            if (page != null && page.Rows.Count > 0)
            {
                if (page.Rows.Count < mapRemittance.BreakPageLine)
                {
                    for (int index = page.Rows.Count; index < mapRemittance.BreakPageLine; index++)
                    {
                        page.Rows.Add("");
                    }
                }
                else
                    page.ToRow = i - 1;

                SetKeyAndOverflowForRemittancePage(page);
                pages.Add(page);
                page = null;
            }

            #endregion

            // Delete INVALID pages
            var pageErrors = pages.Where(e => e.Rows.Count < mapRemittance.DetailFromLine || e.Rows.Count < mapRemittance.LinkFieldLine).ToList();
            foreach (var pageError in pageErrors)
            {
                pages.Remove(pageError);
            }
            return pages;

        }

        private StringBuilder ConvertDataToStandardFile()
        {
            try
            {
                return ConvertDocumentsToStandardFile();
            }
            catch (Exception ex)
            {
                ListErrors.Add(ex.Message);
                return new StringBuilder();
            }
        }

        private void BeforeConvertDataToStandardFile()
        {
            try
            {
                ProcessBeforeConvertDocumentsToStandardFile();
            }
            catch (Exception ex)
            {
                ListErrors.Add(ex.Message);
            }
        }

        protected virtual void ProcessBeforeConvertDocumentsToStandardFile()
        {

        }

        private void ProcessAfterProcessDataFile()
        {
            try
            {
                ProcessAfterConvertDocumentsToStandardFile();
            }
            catch (Exception ex)
            {
                ListErrors.Add(ex.Message);
            }
        }

        protected virtual void ProcessAfterConvertDocumentsToStandardFile()
        {
        }

        protected abstract Map CreateMap();
        protected abstract Document CreateNewDocument();
        protected abstract StringBuilder BuildStandardFileHeader();
        protected abstract StringBuilder ConvertDocumentsToStandardFile();
        #region methods

        protected virtual void SetKeyAndOverflow(Page page)
        {
            // check over
            if (!string.IsNullOrEmpty(Map.MapOverflow?.Key))
            {
                if (page.Rows.Count >= Map.MapOverflow.Line)
                {
                    page.HasOver = page.Rows.Any(e => e.Trim().Contains(Map.MapOverflow.Key.Trim()));
                }
            }

            // set key
            var mapkey = Map.MapHeader.MapFields.FirstOrDefault(e => e.FieldName == Map.MapHeader.FieldKey);
            if (mapkey == null || page.Rows.Count < mapkey.Line) return;
            if (!string.IsNullOrEmpty(Map.Delimiter))
            {
                var rowItems =
                    page.Rows[mapkey.Line - 1].Split(new[] { Map.Delimiter }, StringSplitOptions.None).ToList();
                if (rowItems.Count >= mapkey.Start)
                    page.Key = rowItems[mapkey.Start - 1];
            }
            else page.Key = page.Rows[mapkey.Line - 1].TMid(mapkey.Start, mapkey.Length);
        }
        protected virtual void SetKeyAndOverflowForRemittancePage(Page page)
        {
            var mapRemittance = this.Map.MapRemittance;
            // check over
            if (!string.IsNullOrEmpty(mapRemittance.MapOverflow?.Key))
            {
                if (page.Rows.Count >= mapRemittance.MapOverflow.Line)
                {
                    page.HasOver = page.Rows.Any(e => e.Trim().Contains(mapRemittance.MapOverflow.Key.Trim()));
                }
            }

            // set key
            if (page.Rows.Count < mapRemittance.LinkFieldLine) return;
            if (!string.IsNullOrEmpty(Map.Delimiter))
            {
                var rowItems =
                    page.Rows[mapRemittance.LinkFieldLine - 1].Split(new[] { Map.Delimiter }, StringSplitOptions.None).ToList();
                if (rowItems.Count >= mapRemittance.LinkFieldStart)
                    page.Key = rowItems[mapRemittance.LinkFieldStart - 1];
            }
            else page.Key = page.Rows[mapRemittance.LinkFieldLine - 1].TMid(mapRemittance.LinkFieldStart, mapRemittance.LinkFieldLength);
        }
        internal static bool CheckCondition(List<Condition> conditions, string data)
        {
            if (string.IsNullOrEmpty(data)) return false;
            foreach (var condition in conditions)
            {
                if (condition.Start == 0 && !data.Contains(condition.Text)) return false;
                if (condition.Start <= 0)
                    continue;
                var value = data.Mid(condition.Start, condition.Length);

                switch (condition.Operator)
                {
                    case "==":
                        if (condition.Text != value) return false;
                        break;

                    case "!=":
                        if (condition.Text == value) return false;
                        break;

                    case "Contains":
                        if (!data.Contains(condition.Text)) return false;
                        break;
                }
            }

            return true;

        }

        protected bool ValidateFileFormatBySpecificPos(Page page, List<Condition> conditions,
            string fieldName = "Check_number", string condition = "", string Operator = "==")
        {
            try
            {

                if (!string.IsNullOrEmpty(fieldName))
                {
                    var mapkey = Map.MapHeader.MapFields.FirstOrDefault(e => e.FieldName == fieldName);
                    if (mapkey != null && Pages[0].Rows.Count >= mapkey.Line)
                    {
                        // validate condition
                        switch (Operator)
                        {
                            case "==":
                                if (page.Rows[mapkey.Line - 1].TMid(mapkey.Start, mapkey.Length) == condition)
                                    return true;
                                break;
                            case "!=":
                                if (page.Rows[mapkey.Line - 1].TMid(mapkey.Start, mapkey.Length) != condition)
                                    return true;
                                break;
                        }
                    }
                }
                else
                {
                    // in this case, data line(=Line) contains value need to be checked
                    if (CheckCondition(conditions, page.Rows[conditions[0].Line - 1])) return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }


        #endregion

    }
}