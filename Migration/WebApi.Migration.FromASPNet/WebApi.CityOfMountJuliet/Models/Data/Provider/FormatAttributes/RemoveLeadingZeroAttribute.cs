namespace WebApi.CityOfMountJuliet.Models.Data.Provider.FormatAttributes
{
    internal class RemoveLeadingZeroAttribute : FormatPropertyAttribute
    {
        internal override string Format(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;
            var removed = value.TrimStart('0');
            return removed.Length == 0 ? "0" : removed;
        }
    }
}