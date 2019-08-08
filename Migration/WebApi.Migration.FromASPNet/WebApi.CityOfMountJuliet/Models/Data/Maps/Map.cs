using System;
using System.Collections.Generic;

namespace WebApi.CityOfMountJuliet.Models.Data.Maps
{
    internal abstract class Map
    {
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
        internal List<Condition> BreakPageCondition { get; set; }

        /// <summary>
        /// Dấu hiệu nhận biết bắt đầu 1 page
        /// </summary>
        internal List<Condition> NewPageCondition { get; set; }

        internal List<Condition> ValidatePosition { get; set; }

        /// <summary>
        /// CSV file with delimiter and position of Column
        /// </summary>
        internal string Delimiter { get; set; }
        internal int ColumnNameRow { get; set; }

        internal MapHeader MapHeader { get; set; }
        internal MapDetail MapDetail { get; set; }
        internal MapOverflow MapOverflow { get; set; }

        internal MapRemittance MapRemittance { get; set; }
        /// <summary>
        /// Các filed address
        /// </summary>
        internal List<string> AddressPreFixs { get; set; }

        protected Map()
        {
            this.MapHeader = new MapHeader();
            this.MapDetail = new MapDetail();
            this.AddressPreFixs = new List<string>();
            this.NewPageCondition = new List<Condition>();
            this.BreakPageCondition = new List<Condition>();
            this.ValidatePosition = new List<Condition>();

            this.DataFromLine = 1;
        }

        internal virtual void Initialize()
        {
            try
            {
                this.SetMapHeader();
                this.SetMapDetail();
                this.SetMapRemittance();


                if (MapRemittance != null)
                {
                    if (MapRemittance.MapFields.Count == 0)
                        MapRemittance.MapFields = MapDetail.MapFields;
                    if (MapRemittance.DetailCount == 1)
                        MapRemittance.DetailCount = MapDetail.DetailCount;
                    if (MapRemittance.DetailLength == 0)
                        MapRemittance.DetailLength = MapDetail.DetailLength;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Map file is INVALID: " + ex.Message, ex);
            }
        }

        internal abstract void SetMapHeader();
        internal abstract void SetMapDetail();
        internal virtual void SetMapRemittance() { }
    }
}
