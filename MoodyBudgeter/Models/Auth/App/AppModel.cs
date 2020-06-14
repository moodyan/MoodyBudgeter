using System;

namespace MoodyBudgeter.Models.Auth.App
{
    public class AppModel
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Name { get; set; }
        public bool AllowImplicit { get; set; }
        public bool AllowAuthCode { get; set; }
        public bool AllowClientCredentials { get; set; }
        public string RedirectUri { get; set; }
        public int? UserId { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public string UpdateBy { get; set; }
    }
}
