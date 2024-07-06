using System.ComponentModel.DataAnnotations;

namespace STELA_AUTH.Core.Entities.Request
{
    public class AccountVerificationBody
    {
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; init; }

        [Required]
        [MinLength(4, ErrorMessage = "Min length code must be 4 characters")]
        public string Code { get; init; }
    }
}