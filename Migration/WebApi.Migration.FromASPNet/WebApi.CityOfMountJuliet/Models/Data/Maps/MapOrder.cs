namespace WebApi.CityOfMountJuliet.Models.Data.Maps
{
    internal class MapOrder
    {
        /// <summary>
        /// Field can sap xep
        /// </summary>
        internal string Field { get; set; }

        internal bool Descending { get; set; }

        internal MapOrder(string field, bool descending = false)
        {
            this.Field = field;
            this.Descending = descending;
        }
    }
}

