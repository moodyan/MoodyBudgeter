namespace MoodyBudgeter.Models.User.Search
{
    public class UserSearchResponse
    {
        public int UserId { get; set; }
        public string DisplayName { get; set; }
        public string Avatar { get; set; }
        public string SearchFieldValue { get; set; }
    }
}
