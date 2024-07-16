using System.ComponentModel.DataAnnotations;
using STELA_AUTH.Core.Entities.Response;
using STELA_AUTH.Core.Enums;

namespace STELA_AUTH.Core.Entities.Models
{
    public class Account
    {
        public Guid Id { get; set; }

        [StringLength(256, MinimumLength = 3)]
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? Phone { get; set; }
        public string PasswordHash { get; set; }
        public string? RestoreCode { get; set; }
        public string RoleName { get; set; }
        public DateTime? RestoreCodeValidBefore { get; set; }
        public bool WasPasswordResetRequest { get; set; }
        public string? Token { get; set; }
        public DateTime? TokenValidBefore { get; set; }
        public string? Image { get; set; }
        public string? ConfirmationCode { get; set; }
        public DateTime? ConfirmationCodeValidBefore { get; set; }
        public bool IsEmailVerified { get; set; }
        public DateTime? LastPasswordDateModified { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ProfileBody ToProfileBody()
        {
            return new ProfileBody
            {
                Email = Email,
                Role = Enum.Parse<AccountRole>(RoleName),
                FirstName = FirstName,
                LastName = LastName,
                Phone = Phone,
                IsEmailVerified = IsEmailVerified,
                UrlImage = string.IsNullOrEmpty(Image) ? null : $"{Constants.WebUrlToProfileImage}/{Image}",
            };
        }

        public AccountBody ToAccountBody()
        {
            return new AccountBody
            {
                Id = Id,
                FirstName = FirstName,
                LastName = LastName,
                Email = Email,
                Role = Enum.Parse<AccountRole>(RoleName),
            };
        }
    }
}