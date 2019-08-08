namespace WebApi.CityOfMountJuliet.Models.Data.Maps
{
    internal class Condition
    {
        internal string Text { get; set; }
        internal int Line { get; set; }
        internal int Start { get; set; }
        internal int Length { get; set; }
        internal string Operator { get; set; }
        internal int ElementIndex { get; set; }
        internal Condition(string text, int line = 0, int start = 0, int length = 0, string @operator = "==")
        {
            this.Text = text;
            this.Line = line;
            this.Start = start;
            this.Length = length;
            this.Operator = @operator;
        }
        internal Condition(string text, int elementIndex = 0)
        {
            this.Text = text;
            this.ElementIndex = elementIndex;
        }
    }
}
