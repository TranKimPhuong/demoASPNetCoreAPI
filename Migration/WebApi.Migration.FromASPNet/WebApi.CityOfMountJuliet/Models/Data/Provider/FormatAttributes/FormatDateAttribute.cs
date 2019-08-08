using System;
using System.Globalization;

namespace WebApi.CityOfMountJuliet.Models.Data.Provider.FormatAttributes
{
    /// <summary>
    /// Format value with many format to MM/dd/yyyy format which required for standard file.
    /// </summary>
    internal class FormatDateAttribute : FormatPropertyAttribute
    {
        protected string[] Formats;
        protected string OutputFormat;
        /// <param name="inputFormats">Possible formats of INPUT FILE</param>
        internal FormatDateAttribute(string outputFormat, string[] inputFormats)
        {
            Formats = inputFormats;
            OutputFormat = outputFormat;
        }
        internal override string Format(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;
            DateTime dt;
            if (DateTime.TryParseExact(value, Formats, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out dt))
            {
                return dt.ToString(OutputFormat);
            }
            return value;
        }
    }
}