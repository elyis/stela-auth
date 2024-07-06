using System.ComponentModel.DataAnnotations;

namespace STELA_AUTH.Core.Entities.Request
{
    public class PatchEmailBody
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}