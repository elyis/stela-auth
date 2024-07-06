using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace STELA_AUTH.Core.Entities.Request
{
    public class UpdateAccountCredentialsBody : IValidatableObject
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (FirstName != null && !IsValidName(FirstName))
            {
                yield return new ValidationResult("First name is not valid format (only russian letters)", new[] { nameof(FirstName) });
            }

            if (LastName != null && !IsValidName(LastName))
            {
                yield return new ValidationResult("Last name is not valid format only russian letters)", new[] { nameof(LastName) });
            }
        }

        private bool IsValidName(string name)
        {
            var regex = new Regex(@"^[А-Яа-яёЁ]+$");
            return regex.IsMatch(name);
        }
    }
}