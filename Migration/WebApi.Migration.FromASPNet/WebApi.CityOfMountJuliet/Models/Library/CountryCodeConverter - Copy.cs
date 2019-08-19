using Microsoft.AspNetCore.Http;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace WebApi.CityOfMountJuliet.Models.Library
{
    public sealed class CountryCodeConverter
    {
        [Serializable]
        [XmlType(TypeName = "CountryInfo")]
        public class CountryInfo
        {
            public string Name { get; set; }
            public string TwoLetterCode { get; set; }
            public string ThreeLetterCode { get; set; }
            public string NumericCode { get; set; }

        }

        #region SingleTon
        private static Lazy<CountryCodeConverter> lazy;
        private CountryCodeConverter(IHttpContextAccessor httpContextAccessor)
        {
            var path = httpContextAccessor.HttpContext.Request.Path.Value + "/XMLs/CountryCode.xml";
            if (!File.Exists(path))
            {
                throw new Exception("CountryCodeConverter: Can not find country code dictionary file.");
            }
            var serializer = new XmlSerializer(typeof(CountryInfo[]));
            using (FileStream fileStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Read))
            {
                countries = (CountryInfo[])serializer.Deserialize(fileStream);
            }
        }

        public static void Init(IHttpContextAccessor httpContextAccessor)
        {
            if (lazy == null)
            {
                lazy = new Lazy<CountryCodeConverter>(() => new CountryCodeConverter(httpContextAccessor));
            }
        }

        static CountryCodeConverter _internalInstance { get { return lazy.Value; } }
        #endregion


        CountryInfo[] countries;

        /// <summary>
        /// shorthand
        /// </summary>
        internal static string ConvertToThreeLetterISORegionName(string countryName)
        {
            return _internalInstance.InternalConvert(countryName);
        }

        string InternalConvert(string countryName)
        {
            if (countryName == null)
                return string.Empty;

            countryName = countryName.ToUpper().Trim();

            if (countryName.Length < 2)
            {
                //TODO: log
                return string.Empty;
            }

            if (countryName.Length == 2)
                return Convert2DigitTo3Digit(countryName);

            if (countryName.Length == 3)
                return ThreeLetterISORegionName(countryName);

            return ConvertNameTo3Digit(countryName);
        }
        string ConvertNumericCodeTo3Digit(string code)
        {
            var countryInfo = countries.FirstOrDefault(c =>
              c.NumericCode.ToUpper(CultureInfo.InvariantCulture) == code);
            return countryInfo != null ? countryInfo.ThreeLetterCode : string.Empty;
        }
        string Convert2DigitTo3Digit(string countryName)
        {
            var countryInfo = countries.FirstOrDefault(c =>
                c.TwoLetterCode.ToUpper(CultureInfo.InvariantCulture) == countryName);
            return countryInfo != null ? countryInfo.ThreeLetterCode : string.Empty;
        }

        string ThreeLetterISORegionName(string countryName)
        {
            int tmpCode;
            if (int.TryParse(countryName, out tmpCode))
            {
                return ConvertNumericCodeTo3Digit(countryName);
            }
            var countryInfo = countries.FirstOrDefault(c =>
                c.ThreeLetterCode.ToUpper(CultureInfo.InvariantCulture) == countryName);
            return countryInfo != null ? countryInfo.ThreeLetterCode : string.Empty;
        }

        string ConvertNameTo3Digit(string countryName)
        {
            var countryInfo = countries.FirstOrDefault(c =>
                c.Name.ToUpper(CultureInfo.InvariantCulture) == countryName
               );
            return countryInfo != null ? countryInfo.ThreeLetterCode : string.Empty;
        }
    }
}