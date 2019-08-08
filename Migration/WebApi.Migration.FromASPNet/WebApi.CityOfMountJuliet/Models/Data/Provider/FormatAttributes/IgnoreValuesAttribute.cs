using System.Linq;

namespace WebApi.CityOfMountJuliet.Models.Data.Provider.FormatAttributes
{
    internal class IgnoreValuesAttribute : FormatPropertyAttribute
    {
        string[] _ignoreValues;
        public IgnoreValuesAttribute(params string[] IgnoreValues)
        {
            _ignoreValues = IgnoreValues;
        }
        internal override string Format(string value)
        {
            return _ignoreValues.Any(v => v == value) ? string.Empty : value;
        }
    }
}