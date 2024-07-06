using System.ComponentModel.DataAnnotations;

namespace STELA_AUTH.Core.Entities.Request
{
    public class TokenBody
    {
        [Required]
        public string Value { get; init; }
    }
}