using System.Collections.Generic;

namespace WebApi.CityOfMountJuliet.Models.Data.Maps
{
    internal class MapRemittance
    {
        /// <summary>
        /// Field dung lam Key de nhan biet remitance thuoc hearder nao
        /// </summary>
        internal string LinkFieldName { get; set; }

        /// <summary>
        /// Field nhan biet nam o line nao trong remittance
        /// </summary>
        internal int LinkFieldLine { get; set; }

        /// <summary>
        /// Field nhan biet bat dau o cot thu may remittance
        /// </summary>
        internal int LinkFieldStart { get; set; }

        /// <summary>
        /// Field nhan biet cua remittance co do dai bao nhieu
        /// </summary>
        internal int LinkFieldLength { get; set; }

        /// <summary>
        /// Bắt đầu đọc dữ liệu từ dòng thứ mấy trog file
        /// </summary>
        internal int DataFromLine { get; set; }

        /// <summary>
        /// Vị trí dòng kết thúc 1 page
        /// </summary>
        internal int BreakPageLine { get; set; }

        /// <summary>
        /// Dấu hiệu nhận biết kết thúc 1 page
        /// </summary>
        internal List<Condition> BreakPageCondition { get; set; } = new List<Condition>();

        /// <summary>
        /// Dấu hiệu nhận biết bắt đầu 1 page
        /// </summary>
        internal List<Condition> NewPageCondition { get; set; } = new List<Condition>();

        /// <summary>
        /// Line bat dau cua cac detail line trong remittance
        /// </summary>
        internal int DetailFromLine { get; set; }

        /// <summary>
        /// Line ket thuc cua cac detail line trong remittance
        /// </summary>
        internal int DetailToLine { get; set; }

        internal int DetailLength { get; set; }
        internal int DetailCount { get; set; }

        /// <summary>
        /// Chuoi danh dau het 1 list detail line
        /// </summary>
        internal List<Condition> BreakDetailLineCondition { get; set; } = new List<Condition>();
        internal List<Condition> RecognizeDetailLineCondition { get; set; } = new List<Condition>();

        /// <summary>
        /// Nếu null thì lấy theo file chính (MapDetail)
        /// </summary>
        internal IList<MapField> MapFields { get; set; } = new List<MapField>();
        internal MapOverflow MapOverflow { get; set; }
        internal MapRemittance()
        {

        }
        internal void AddField(string fieldName, int start, int length, string startWith = "", string columnName = "")
        {
            var map = new MapField(fieldName, start, length, startWith, columnName);
            this.MapFields.Add(map);
        }

        internal MapRemittance(
            string linkFieldName,
            int linkFieldLine, int linkfieldStart, int linkfieldLength,
            int detailFromLine, int detailToLine = 0,
            int detailLength = 0, int detailCount = 1,
            string breakDetailLineKey = "", int breakDetailLineKeyStart = 0, int breakDetailLineKeyLegnth = 0,
            int dataFromLine = 1,
            int breakPageLine = 0, string breakPageKey = "", int breakPageKeyStart = 0, int breakPageKeyLegnth = 0)
        {
            this.LinkFieldName = linkFieldName;
            this.LinkFieldStart = linkfieldStart;
            this.LinkFieldLength = linkfieldLength;
            this.LinkFieldLine = linkFieldLine;
            this.DetailFromLine = detailFromLine;
            this.DetailToLine = detailToLine;
            this.DetailLength = detailLength;
            this.DetailCount = detailCount;

            this.DataFromLine = dataFromLine;
            this.BreakPageLine = breakPageLine;

        }
    }
}
