using System.ComponentModel.DataAnnotations;

namespace STELA_AUTH.Core.Entities.Request
{
    public class SignInBody
    {
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; init; }

        [Required]
        [MinLength(3, ErrorMessage = "Min length password must be 3 characters")]
        public string Password { get; init; }
    }
}