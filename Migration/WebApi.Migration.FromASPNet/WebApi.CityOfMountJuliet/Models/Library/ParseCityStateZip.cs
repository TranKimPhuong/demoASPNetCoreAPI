using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;

namespace WebApi.CityOfMountJuliet.Models.Library
{
    internal static class ParseCityStateZip
    {
        //private static IHostingEnvironment _hostingEnvironment;
        static ParseCityStateZip()
        {
            provinces = GetProvinces();
        }

        static List<Province> provinces;
        static List<Province> GetProvinces(string country = "")
        {
            List<Province> result;
            //TODO: test _hostingEnvironment.ContentRootPath

            string value = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "~/XMLs/provinces.json"));

            result = JsonConvert.DeserializeObject<List<Province>>(value);

            if (string.IsNullOrEmpty(country))
                return result;

            return result.Where(o => o.Country == country).ToList();
        }

        internal static bool TryParseCityStateZip(this string value, out string city, out string state, out string zip)
        {

            city = state = zip = string.Empty;
            if (string.IsNullOrEmpty(value))
                return false;

            var caZip = Regex.Match(value, caZipRegEx);
            if (caZip.Success)
            {
                value = value.Replace(caZip.Value, string.Empty);
                zip = caZip.Value;
            }
            var datas = value.Trim().Replace(",", " ").Replace("  ", " ").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            string lastValue = datas.LastOrDefault()?.Trim();

            //check ZIP
            if (!string.IsNullOrEmpty(lastValue) && ValidateUSorCanadianZipCode(lastValue))
            {
                zip = lastValue;
                datas.RemoveAt(datas.Count - 1);
            }

            ////check STATE
            lastValue = datas.LastOrDefault();

            if (!string.IsNullOrEmpty(lastValue) && provinces.Any(o => o.Short?.ToUpper() == lastValue.ToUpper()))
            {
                state = lastValue.ToUpper();
                datas.RemoveAt(datas.Count - 1);
            }
            if (!string.IsNullOrEmpty(lastValue) && provinces.Any(o => o.Name.ToUpper() == lastValue.ToUpper()))
            {
                state = provinces.FirstOrDefault(s => s.Name.ToUpper() == lastValue.ToUpper())?.Short;
                datas.RemoveAt(datas.Count - 1);
            }
            if (datas.Count > 0)
                city = string.Join(" ", datas)?.Trim();

            return !string.IsNullOrEmpty(city) &&
                !string.IsNullOrEmpty(state) || !string.IsNullOrEmpty(zip);
        }
        static string usZipRegEx = @"\d{5}([ \-]\d{4})?";
        static string caZipRegEx = @"[ABCEGHJKLMNPRSTVXY]\d[ABCEGHJ-NPRSTV-Z][ ]?\d[ABCEGHJ-NPRSTV-Z]\d";

        internal static bool ValidateUSorCanadianZipCode(string zipCode)
        {
            zipCode = zipCode.ToUpper();
            if ((!Regex.Match(zipCode, usZipRegEx).Success) && (!Regex.Match(zipCode, caZipRegEx).Success))
                return false;
            return true;
        }
        class Province
        {
            public string Short { get; set; }
            public string Name { get; set; }
            public string Country { get; set; }
        }
    }


}
