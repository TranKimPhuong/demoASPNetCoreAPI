namespace WebApi.CityOfMountJuliet.Models.Data.Maps
{
    internal class MapOverflow
    {
        internal string Key { get; set; }
        internal int Line { get; set; }
        internal bool HeaderInFirstPage { get; set; }

        internal MapOverflow(string key, int line = 0, bool headerInFirstPage = false)
        {
            this.Key = key;
            this.Line = line;
            this.HeaderInFirstPage = headerInFirstPage;
        }
    }
}
