using System.ComponentModel.DataAnnotations;
using STELA_AUTH.Core.Enums;

namespace STELA_AUTH.Core.Entities.Request
{
    public class PatchAccountCredentialsBody
    {
        public Guid Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        [EnumDataType(typeof(AccountRole))]
        public AccountRole? Role { get; set; }
    }
}