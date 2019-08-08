namespace WebApi.CityOfMountJuliet.Models.Data.Maps
{
    internal class MapField
    {
        internal string FieldName { get; set; }
        internal int ElementIndex { get; set; }

        /// <summary>
        /// Dữ liệu cần lấy ở dòng có bắt đầu bằng StartWith
        /// </summary>
        internal string StartWith { get; set; }

        /// <summary>
        /// Dữ liệu cần lấy ở cột có Column Name
        /// </summary>
        internal string ColumnName { get; set; }

        /// <summary>
        /// vị trí cột hoặc vị trí trên mảng
        /// </summary>
        internal int Start { get; set; }

        /// <summary>
        /// độ dài chuỗi giá trị
        /// </summary>
        internal int Length { get; set; }

        internal MapField() { }
        internal MapField(string fieldName, int elementIndex)
        {
            FieldName = fieldName;
            this.ElementIndex = elementIndex;
        }
        internal MapField(string fieldName, int start, int length, string startWith = "", string columnName = "")
        {
            this.FieldName = fieldName;
            this.Start = start;
            this.Length = length;
            this.StartWith = startWith;
            this.ColumnName = columnName;
        }
    }
}