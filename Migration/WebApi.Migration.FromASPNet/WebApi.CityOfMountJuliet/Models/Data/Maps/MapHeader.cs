using System.Collections.Generic;

namespace WebApi.CityOfMountJuliet.Models.Data.Maps
{
    internal class MapHeader
    {
        internal string FieldKey { get; set; }
        internal IList<MapFieldHeader> MapFields { get; set; }
        internal MapHeader()
        {
            MapFields = new List<MapFieldHeader>();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="line"></param>
        /// <param name="start"></param>
        /// <param name="length">EOL: int.MaxValue - start</param>
        /// <param name="startWith">dùng để nhận biết dòng nào là header bằng một chuỗi nào đó</param>
        internal void AddField(string fieldName, int line, int start, int length, string startWith = "")
        {
            var map = new MapFieldHeader(fieldName, line, start, length, startWith);
            MapFields.Add(map);
        }
        internal void AddField(string fieldName, int elementIndex)
        {
            var map = new MapFieldHeader(fieldName, elementIndex);
            MapFields.Add(map);
        }
    }
}
