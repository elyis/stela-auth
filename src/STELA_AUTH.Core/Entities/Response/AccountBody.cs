using STELA_AUTH.Core.Enums;

namespace STELA_AUTH.Core.Entities.Response
{
    public class AccountBody
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public AccountRole Role { get; set; }
    }
}