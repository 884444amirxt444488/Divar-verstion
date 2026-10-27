namespace Shop_project_cs.Dtos.AuthDtos;

using System.ComponentModel.DataAnnotations;


public class EditDto
{
    [StringLength(8, MinimumLength =4, ErrorMessage ="Username must be betweem 4, 8 character")]
    public string Username {get; set;} = string.Empty;
    [StringLength(32, MinimumLength =4, ErrorMessage ="Email must be between 4, 32 character")]
    [EmailAddress]
    public string Email {get; set;} = string.Empty;
 
    public IEnumerable<ValidationResult> Validation(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Username) && string.IsNullOrWhiteSpace(Email))
        {
            yield return new ValidationResult(
                "Username or emil is required at least",
                new[] {nameof(Username), nameof(Email)}
            );
        }
    }
}







