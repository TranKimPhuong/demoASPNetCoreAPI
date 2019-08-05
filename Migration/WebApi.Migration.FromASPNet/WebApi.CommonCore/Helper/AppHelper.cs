using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text.RegularExpressions;

namespace WebApi.CommonCore.Helper
{
    public static class AppHelper
    {
        public static string CSV_QUOTE = "\"";
        public static string CSV_ESCAPED_QUOTE = "\"\"";
        public static char[] CSV_CHARACTERS_THAT_MUST_BE_QUOTED = {',', '"', '\n' };
         public static string[] DATE_FORMAT = new string[] { "MM/dd/yyyy", "M/dd/yyyy", "MM/d/yyyy", "M/d/yyyy", "yyyy,M,d",
                                                            "yyyy,MM,dd", "yyyy,MM,d", "yyyy,M,dd", "MMddyy", "Mddyy" };

        #region Extension Methods
        /// <summary>
        /// Chunked list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="chunkSize"></param>
        /// <returns></returns>
        public static IEnumerable<List<T>> Chunked<T>(this List<T> source, int chunkSize)
        {
            var offset = 0;
            while (offset < source.Count)
            {
                yield return source.GetRange(offset, Math.Min(source.Count - offset, chunkSize));
                offset += chunkSize;
            }
        }

        public static string EscapeCSV(this string value)
        {
            if (value == null) return string.Empty;

            string returnValue = value;
            if (returnValue.Contains(CSV_QUOTE)) returnValue = returnValue.Replace(CSV_QUOTE, CSV_ESCAPED_QUOTE);
            if (returnValue.IndexOfAny(CSV_CHARACTERS_THAT_MUST_BE_QUOTED) > -1)
                returnValue = CSV_QUOTE + returnValue + CSV_QUOTE;
            return returnValue;
        }

        public static DateTime? ToDate(this string value, params string[] format)
        {
            DateTime? result = null;
            try
            {
                if (!string.IsNullOrEmpty(value))
                {
                    if (format == null || format.Length == 0)
                    {
                        result = DateTime.ParseExact(value, DATE_FORMAT, null, System.Globalization.DateTimeStyles.None);
                    }
                    else
                    {
                        result = DateTime.ParseExact(value, format, null, System.Globalization.DateTimeStyles.None);
                    }
                }
            }
            catch
            {
                //Console.Write(ex);
            }
            return result;
        }

        /// <summary>
        /// Truncates the string.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="maxLength"></param>
        /// <returns></returns>
        public static string Left(this string input, int maxLength)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            if (input.Length <= maxLength) return input.Trim();

            string output = input.Substring(0, maxLength);
            return output.Trim();
        }

        /// <summary>
        /// Gets the right side of the string.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string Right(this string str, int length)
        {
            if (str == null) return null;
            if (str.Length <= length) return str;
            return str.Substring(str.Length - length);
        }

        /// <summary>
        /// Determines if a string consists of all valid ASCII values.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsAscii(this string str)
        {
            if (string.IsNullOrEmpty(str)) return true;

            foreach (var ch in str)
                if ((int)ch > 127) return false;

            return true;
        }

        public static int CountLinesInString(this string str)
        {
            if (str == null) throw new ArgumentNullException("String is NULL");

            int counter = 1;
            string[] strTemp = str.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            if (strTemp.Length > 0)
            {
                counter = strTemp.Length;
            }
            return counter;
        }

        public static bool IsValidOneWord(this string iString)
        {
            if (!String.IsNullOrEmpty(iString))
            {
                string word = iString.Trim().ToLower();
                if (!String.IsNullOrEmpty(word))
                {
                    foreach (char c in word)
                    {
                        if (char.IsLetter(c) == false)
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        #endregion

        private static string _CURRENT_IP = null;
        public static string GetIpAddess()
        {
            try
            {
                if (string.IsNullOrEmpty(_CURRENT_IP))
                {
                    //This returns the first IP4 address or null                
                    IPAddress oIPAddress = Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
                    if (oIPAddress != null) _CURRENT_IP = oIPAddress.ToString();
                    else _CURRENT_IP = Dns.GetHostName();
                }
            }
            catch (Exception ex) { Console.Write(ex.Message); }

            return _CURRENT_IP;
        }

        public static string GetErrorMessage(Exception ex)
        {
            if (ex == null) return string.Empty;

            string error = string.Empty;
            if (ex.InnerException == null)
            {
                error = ex.Message;
            }
            else
            {
                error = ex.InnerException.Message;
            }
            return error;
        }

        public static bool IsLocalEzitIpAddress(string host)
        {
            try
            {
                string testHostName = Dns.GetHostName();
                if (Regex.IsMatch(testHostName, @"EZIT*", RegexOptions.IgnoreCase)) return true;

                // get host IP addresses
                IPAddress[] hostIPs = Dns.GetHostAddresses(host);
                // get local IP addresses
                IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());

                // test if any host IP equals to any local IP or to localhost
                foreach (IPAddress hostIP in hostIPs)
                {
                    // is localhost
                    if (IPAddress.IsLoopback(hostIP)) return true;
                    // is local address
                    foreach (IPAddress localIP in localIPs)
                    {
                        if (hostIP.Equals(localIP)) return true;
                    }
                }
            }
            catch { }
            return false;
        }


        /// <summary>
        /// Convert the list to DataTable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <returns></returns>
        public static DataTable ToDataTable<T>(IList<T> items)
        {
            if (items == null) return null;
            DataTable dataTable = new DataTable();

            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.DeclaredOnly
                                                | BindingFlags.Instance
                                                | BindingFlags.Public
                                                | BindingFlags.GetField
                                                | BindingFlags.SetField);
            foreach (PropertyInfo prop in Props)
            {
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            //put a breakpoint here and check datatable
            return dataTable;
        }

        public static string ToString(string value)
        {
            string result = string.Empty;
            try
            {
                if (value != null) result = Convert.ToString(value);
            }
            catch (Exception ex) { Console.Write(ex); }
            return result.Trim();
        }

        public static string ToString(object value)
        {
            string result = string.Empty;
            try
            {
                if (value != null) result = Convert.ToString(value);
            }
            catch (Exception ex) { Console.Write(ex); }
            return result.Trim();
        }

        public static int ToInt(object value)
        {
            int result = 0;
            try
            {
                if (value != null) result = Convert.ToInt32(value);
            }
            catch (Exception ex) { Console.Write(ex); }
            return result;
        }

        public static bool IsNumeric(object Expression)
        {
            double retNum = 0.0;
            return IsNumeric(Expression, ref retNum);
        }

        public static bool IsNumeric(object Expression, ref double retNum)
        {
            bool isNum = false;
            try
            {
                string expressionValue = ToString(Expression);
                if (!"NaN".Equals(expressionValue, StringComparison.OrdinalIgnoreCase))
                {
                    isNum = Double.TryParse(Convert.ToString(Expression), System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out retNum);
                }
            }
            catch (Exception ex) { Console.WriteLine(ex); }

            return isNum;
        }

        public static bool IsNumeric(object Expression, ref decimal retNum)
        {
            bool isNum = false;
            try
            {
                isNum = Decimal.TryParse(Convert.ToString(Expression), System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out retNum);
            }
            catch (Exception ex) { Console.WriteLine(ex); }

            return isNum;
        }

       
    }
}