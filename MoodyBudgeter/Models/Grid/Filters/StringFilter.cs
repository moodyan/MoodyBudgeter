namespace MoodyBudgeter.Models.Grid.Filters
{
    public class StringFilter
    {
        public string FieldName { get; set; }
        public StringOperator Operator { get; set; }
        public string Value { get; set; }
    }

    public enum StringOperator
    {
        EqualTo,
        Contains,
        StartsWith,
        EndsWith,
        NotEqual,
        NotIn
    }
}
