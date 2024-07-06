using System.ComponentModel.DataAnnotations;

namespace STELA_AUTH.Core.Entities.Request
{
    public class SignUpBody
    {
        [Required]
        [RegularExpression(@"^[А-Яа-яёЁ]+$", ErrorMessage = "First name must contain only ru letters")]
        public string FirstName { get; init; }

        [Required]
        [RegularExpression(@"^[А-Яа-яёЁ]+$", ErrorMessage = "Last name must contain only ru letters")]
        public string LastName { get; init; }

        [Required]
        [EmailAddress]
        public string Email { get; init; }

        [Required]
        [MinLength(3, ErrorMessage = "Min length password must be 3 characters")]
        public string Password { get; init; }
    }
}