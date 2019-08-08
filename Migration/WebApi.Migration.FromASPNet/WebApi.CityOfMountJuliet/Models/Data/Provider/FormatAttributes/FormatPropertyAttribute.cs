using System;

namespace WebApi.CityOfMountJuliet.Models.Data.Provider.FormatAttributes
{
    [AttributeUsage(AttributeTargets.Property)]
    internal abstract class FormatPropertyAttribute : Attribute
    {
        internal abstract string Format(string value);
    }
}