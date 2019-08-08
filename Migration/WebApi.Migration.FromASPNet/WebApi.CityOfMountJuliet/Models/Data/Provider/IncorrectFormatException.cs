using System;

namespace WebApi.CityOfMountJuliet.Models.Data.Provider
{
    internal class IncorrectFormatException : Exception
    {
        internal IncorrectFormatException(string message) : base(message)
        {
        }
    }
}