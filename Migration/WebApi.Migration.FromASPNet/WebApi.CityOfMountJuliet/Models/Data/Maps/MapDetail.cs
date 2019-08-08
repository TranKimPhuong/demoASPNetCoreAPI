using System.Collections.Generic;

namespace WebApi.CityOfMountJuliet.Models.Data.Maps
{
    internal class MapDetail
    {
        internal int FromLine { get; set; } = 1;
        internal int ToLine { get; set; } = 0;
        internal int DetailLength { get; set; }
        internal int DetailCount { get; set; } = 1;

        internal List<Condition> BreakDetailLineCondition { get; set; } = new List<Condition>();
        internal List<Condition> RecognizeDetailLineCondition { get; set; } = new List<Condition>();

        internal IList<MapField> MapFields { get; set; } = new List<MapField>();

        internal MapDetail()
        {
        }

        internal MapDetail(int fromLine, int toLine, int detailLength = 0, int detailCount = 1)
        {
            this.FromLine = fromLine;
            this.ToLine = toLine;
            this.DetailLength = detailLength;
            this.DetailCount = detailCount;
        }

        internal void AddField(string fieldName, int start, int length, string startWith = "", string columnName = "")
        {
            var map = new MapField(fieldName, start, length, startWith, columnName);
            this.MapFields.Add(map);
        }
        internal void AddField(string fieldName, int elementIndex)
        {
            var map = new MapField(fieldName, elementIndex);
            MapFields.Add(map);
        }
    }
}
