namespace MoodyBudgeter.Models.Grid.Filters
{
    public class NumberFilter
    {
        public string FieldName { get; set; }
        public NumberOperator Operator { get; set; }
        public double Value { get; set; }
    }

    public enum NumberOperator
    {
        EqualTo,
        LessThan,
        GreaterThan,
        NotEqual
    }
}
