using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ExcelDataReader;

namespace WebApi.CityOfMountJuliet.Models.Library
{
    internal static class StringExtensions
    {
        internal static string Mid(this string sText, int iPos, int iLen = 0)
        {
            if (iPos < 0) return string.Empty;
            if (iPos > sText.Length) return string.Empty;
            if (iPos == 0) iPos = 1;

            if (iPos + iLen > sText.Length || iLen == 0)
                return sText.Substring(iPos - 1);

            return sText.Substring(iPos - 1, iLen);
        }
        internal static string TMid(this string text, int piPos, int piLen)
        {
            if (piPos < 0)
                return string.Empty;
            if (piPos > text.Length)
                return string.Empty;
            if (piPos == 0) piPos = 1;
            if (piLen == 0 || piPos + piLen > text.Length)
                return text.Substring(piPos - 1).Trim();
            return text.Substring(piPos - 1, piLen).Trim();
        }
        internal static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }
            value = value.Trim();
            if (value.Length > maxLength)
            {
                return value.Substring(0, maxLength);
            }
            return value;
        }
        internal static List<string> LoadListFields(this string psStringInput, string psDelimeter, string psQuote)
        {
            List<string> lstFieldData = new List<string>();

            int iSPos = 0;
            int iDelimPos = 0;
            int iLen = 0;
            int iFieldCount = 0;
            int iQuotePos = 0;

            string Quote = "";
            string Delimeter = "";
            try
            {
                if (psDelimeter != "") Delimeter = psDelimeter;
                else Delimeter = ",";
                if (psQuote != "") Quote = psQuote;
                else Quote = "\"";
                do
                {
                    if (psStringInput.Substring(iSPos, 1) == Quote)
                    {
                        iQuotePos = psStringInput.IndexOf(Quote, iSPos + 1);
                        if (iQuotePos < psStringInput.Length - 1)
                        {
                            if (psStringInput.Substring(iQuotePos + 1, 1) == Quote)
                            {
                                do
                                {
                                    if (psStringInput.IndexOf(Quote, iQuotePos + 2) >= 0)
                                    {
                                        iQuotePos = psStringInput.IndexOf(Quote, psStringInput.IndexOf(Quote, iQuotePos + 2));
                                        if (psStringInput.IndexOf(Delimeter, iQuotePos + 1) >= 0)
                                        {
                                            iDelimPos = psStringInput.IndexOf(Delimeter, iQuotePos + 1);
                                        }
                                        else
                                        {
                                            iDelimPos = psStringInput.Length;
                                        }
                                    }
                                    else if (psStringInput.IndexOf(Quote, iQuotePos) == -1)
                                    {
                                        iQuotePos = iQuotePos + 1;
                                        if (psStringInput.IndexOf(Delimeter, iQuotePos + 1) >= 0)
                                        {
                                            iDelimPos = psStringInput.IndexOf(Delimeter, iQuotePos + 1);
                                        }
                                        else
                                        {
                                            iDelimPos = psStringInput.Length;
                                        }
                                    }
                                    else
                                    {
                                        iQuotePos = psStringInput.IndexOf(Quote, iQuotePos + 2);
                                        if (psStringInput.IndexOf(Delimeter, iQuotePos + 1) >= 0)
                                        {
                                            iDelimPos = psStringInput.IndexOf(Delimeter, iQuotePos + 1);
                                        }
                                        else
                                        {
                                            iDelimPos = psStringInput.Length;
                                        }
                                    }
                                } while (psStringInput.Substring(iQuotePos + 1, 1) == Quote);
                            }
                            else if (psStringInput.Substring(iQuotePos + 1, 1) == Delimeter)
                            {
                                iDelimPos = iQuotePos + 1;
                            }
                            else
                            {
                                if (psStringInput.IndexOf(Delimeter, iQuotePos + 1) >= 0)
                                {
                                    iDelimPos = psStringInput.IndexOf(Delimeter, iQuotePos + 1);
                                }
                                else
                                {
                                    iDelimPos = psStringInput.Length;
                                }
                            }
                        }
                        else
                        {
                            iDelimPos = psStringInput.Length;
                        }
                    }
                    else
                    {
                        iDelimPos = psStringInput.IndexOf(Delimeter, iSPos);
                        if (iDelimPos == -1) iDelimPos = psStringInput.Length;
                    }

                    if (iDelimPos == 41)
                    {
                        iLen = iFieldCount;
                    }

                    iLen = iDelimPos - iSPos;
                    lstFieldData.Add("");

                    if (iLen > 0)
                    {
                        lstFieldData[iFieldCount] = psStringInput.Substring(iSPos, iLen).Trim();
                    }

                    if (lstFieldData[iFieldCount].Trim() != "" && lstFieldData[iFieldCount].Length >= 1)
                    {
                        if (lstFieldData[iFieldCount].Substring(0, 1) == Quote &&
                            lstFieldData[iFieldCount].Substring(lstFieldData[iFieldCount].Length - 1, 1) == Quote)
                        {
                            if (lstFieldData[iFieldCount].Length >= 2)
                            {
                                lstFieldData[iFieldCount] = lstFieldData[iFieldCount].Substring(1,
                                                                                                lstFieldData[iFieldCount
                                                                                                    ].Length - 2).Trim();
                            }
                        }
                    }

                    // Control special data
                    if (lstFieldData[iFieldCount].IndexOf("\"\"\"\"", 0) >= 0)
                    {
                        lstFieldData[iFieldCount] = lstFieldData[iFieldCount].Replace("\"\"\"\"", "\"\"");
                    }
                    if (lstFieldData[iFieldCount].IndexOf("\"\"", 0) >= 0)
                    {
                        lstFieldData[iFieldCount] = lstFieldData[iFieldCount].Replace("\"\"", "\"");
                    }
                    iFieldCount = iFieldCount + 1;
                    iSPos = iDelimPos + 1;

                } while (iSPos < psStringInput.Length);

                if (iSPos == psStringInput.Length) lstFieldData.Add("");
            }
            catch (Exception ex)
            {
                throw new Exception("The file could not be read:" + ex.Message + "\r\n");
            }

            return lstFieldData;
        }
    }
    internal static class IEnumerableExtensions
    {
        internal static DataTable ToDataTable<T>(this IEnumerable<T> data)
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));
            var table = new DataTable();
            foreach (PropertyDescriptor prop in properties)
            {
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            }

            foreach (T item in data)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                {
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;

                }
                table.Rows.Add(row);
            }

            return table;
        }
        internal static void RemoveRange<T>(this IList<T> iList, IEnumerable<T> itemsToRemove)
        {
            var set = new HashSet<T>(itemsToRemove);

            var list = iList as List<T>;
            if (list == null)
            {
                int i = 0;
                while (i < iList.Count)
                {
                    if (set.Contains(iList[i])) iList.RemoveAt(i);
                    else i++;
                }
            }
            else
            {
                list.RemoveAll(set.Contains);
            }
        }
        internal static IEnumerable<TSource> DistinctBy<TSource, TKey>
   (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }

    }
    internal static class IQueryableExtensions
    {
        private static IOrderedQueryable<T> OrderingHelper<T>(IQueryable<T> source, string propertyName, bool descending, bool anotherLevel)
        {
            var param = Expression.Parameter(typeof(T), "p");
            var property = Expression.PropertyOrField(param, propertyName);
            var sort = Expression.Lambda(property, param);

            var call = Expression.Call(
                typeof(Queryable),
                (!anotherLevel ? "OrderBy" : "ThenBy") + (descending ? "Descending" : string.Empty),
                new[] { typeof(T), property.Type },
                source.Expression,
                Expression.Quote(sort));

            return (IOrderedQueryable<T>)source.Provider.CreateQuery<T>(call);
        }

        internal static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, string propertyName, bool descending = false)
        {
            return OrderingHelper(source, propertyName, descending, false);
        }

        internal static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> source, string propertyName, bool descending = false)
        {
            return OrderingHelper(source, propertyName, descending, true);
        }
    }
    internal static class DataTableExtensions
    {

        internal static List<T> ToList<T>(this DataTable table) where T : class, new()
        {
            try
            {
                List<T> list = new List<T>();
                using (IEnumerator<DataRow> enumerator = table.Select().AsEnumerable().GetEnumerator())
                {
                    while (enumerator?.MoveNext() ?? false)
                    {
                        T item = enumerator.Current.ToObject<T>();
                        list.Add(item);
                    }
                }
                return list;
            }
            catch
            {
                return null;
            }
        }
        internal static T ToObject<T>(this DataRow row) where T : class, new()
        {
            T local = Activator.CreateInstance<T>();
            foreach (PropertyInfo info in local.GetType().GetProperties())
            {
                try
                {
                    if (info.PropertyType.IsGenericType && info.PropertyType.Name.Contains("Nullable"))
                    {
                        if (!string.IsNullOrEmpty(row[info.Name].ToString()))
                        {
                            info.SetValue(local, Convert.ChangeType(row[info.Name], Nullable.GetUnderlyingType(info.PropertyType), null));
                        }
                    }
                    else
                    {
                        info.SetValue(local, Convert.ChangeType(row[info.Name], info.PropertyType), null);
                    }
                }
                catch
                {
                }
            }
            return local;
        }
    }
    internal static class ByteArrayExtensions
    {
        /// <summary>
        /// Get data from Excel-formatted file.
        /// </summary>
        /// <param name="byteArr">The excel file content in byte[]</param>
        /// <param name="sheetIndex">The sheet index (zero-based).</param>
        /// <returns></returns>
        internal static DataTable ReadDataFromExcel(this byte[] byteArr, int sheetIndex = 0)
        {
            if (sheetIndex < 0)
                throw new ArgumentException($"sheetIndex must be equal or greater than 0");
            using (var mStream = new MemoryStream(byteArr))
            using (var excelReader = ExcelReaderFactory.CreateReader(mStream))
            {
                var dataSet = excelReader.AsDataSet();
                var table = dataSet.Tables.Count > sheetIndex ? dataSet.Tables[sheetIndex] : null;
                return table;
            }
        }
    }
}
