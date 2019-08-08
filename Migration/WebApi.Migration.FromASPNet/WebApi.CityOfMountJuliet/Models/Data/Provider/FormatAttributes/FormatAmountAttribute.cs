namespace WebApi.CityOfMountJuliet.Models.Data.Provider.FormatAttributes
{
    internal class FormatAmountAttribute : FormatPropertyAttribute
    {
        internal override string Format(string value)
        {
            var tempString = value.Replace("$", string.Empty).Replace("*", string.Empty);
            decimal dec;
            if (decimal.TryParse(tempString, out dec))
                return dec.ToString("0.00");
            return value;
        }
    }
}