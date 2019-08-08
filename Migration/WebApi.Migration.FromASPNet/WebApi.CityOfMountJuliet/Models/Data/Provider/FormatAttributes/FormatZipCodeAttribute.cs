using System.Text.RegularExpressions;

namespace WebApi.CityOfMountJuliet.Models.Data.Provider.FormatAttributes
{
    internal class FormatZipCodeAttribute : FormatPropertyAttribute
    {
        internal override string Format(string value)
        {
            if (!value.Contains("-") && value.Length > 7 && Regex.Matches(value, @"[a-zA-Z]").Count == 0)
            {
                value = value.Insert(5, "-");
            }
            return value;
        }
    }
}