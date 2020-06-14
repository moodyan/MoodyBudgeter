namespace MoodyBudgeter.Models.User.Search
{
    public class UserSearch
    {
        public string SearchText { get; set; }
        public string ProfilePropertyName { get; set; }
        public int? ProfilePropertyId { get; set; }
        public bool SearchUsername { get; set; }
        public SearchOperator Operator { get; set; }
        public int PageSize { get; set; }
        public int PageOffset { get; set; }
        public bool SortAscending { get; set; }
        public string SortField { get; set; }
        public bool IsAdmin { get; set; }
        public bool IncludeAvatar { get; set; }
    }
}
