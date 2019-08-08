using WebApi.CityOfMountJuliet.Models.Library;

namespace WebApi.CityOfMountJuliet.Models.Data.Provider.FormatAttributes
{
    internal class ConvertCountryCodeAttribute : FormatPropertyAttribute
    {
        internal override string Format(string value)
        {
            return CountryCodeConverter.ConvertToThreeLetterISORegionName(value);
        }
    }
}