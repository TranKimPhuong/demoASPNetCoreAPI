namespace WebApi.CityOfMountJuliet.Models.Data.Maps
{
    internal class MapFieldHeader : MapField
    {

        /// <summary>
        /// Dữ liệu cần lấy ở dòng thứ mấy
        /// </summary>
        internal int Line { get; set; }

        internal MapFieldHeader(string fieldName, int line, int start, int length, string startWith = "")
        {
            this.FieldName = fieldName;
            this.Line = line;
            this.Start = start;
            this.Length = length;
            this.StartWith = startWith;
        }

        internal MapFieldHeader(string fieldName, int elementIndex) : base(fieldName, elementIndex)
        {
        }
    }
}
