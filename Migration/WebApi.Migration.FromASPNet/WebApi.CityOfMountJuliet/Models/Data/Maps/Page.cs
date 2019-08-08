using System.Collections.Generic;

namespace WebApi.CityOfMountJuliet.Models.Data.Maps
{
    internal class Page
    {
        /// <summary>
        /// Key in Page: nên dùng check number làm key
        /// Key in Remittance Page: là field map giữ file chính và file remittance
        /// </summary>
        internal string Key { get; set; }
        internal bool HasOver { get; set; }
        internal List<string> Rows { get; set; }
        internal int FromRow { get; set; }
        internal int ToRow { get; set; }
        internal string ColumnNameData { get; set; }

        internal Page()
        {
            this.Rows = new List<string>();
        }
    }
}
