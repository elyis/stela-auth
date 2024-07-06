using STELA_AUTH.Core.Enums;

namespace STELA_AUTH.Core.Entities.Response
{
    public class ProfileBody
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? Phone { get; set; }
        public AccountRole Role { get; set; }
        public bool IsEmailVerified { get; set; }
        public string? UrlImage { get; set; }
    }
}