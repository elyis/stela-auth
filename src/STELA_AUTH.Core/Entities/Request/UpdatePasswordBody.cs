using System.ComponentModel.DataAnnotations;

namespace STELA_AUTH.Core.Entities.Request
{
    public class UpdatePasswordBody
    {
        [Required]
        [MinLength(3, ErrorMessage = "Min length password must be 3 characters")]
        public string Password { get; init; }

        [Required]
        [MinLength(3, ErrorMessage = "Min length new password must be 3 characters")]
        public string NewPassword { get; init; }
    }
}