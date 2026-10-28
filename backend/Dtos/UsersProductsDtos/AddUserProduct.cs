

namespace  Shop_project_cs.Dtos.UsersProductsDtos;

using System.ComponentModel.DataAnnotations;

public class AddUserProductDto
{
    [Required]
    [Range(1, 1000000000)]
    public int UserId {get; set;} = 0;
    [Required]
    [Range(typeof(decimal), "0.01", "9999999999999999")]
    public decimal Price {get; set;} = 0;
    [Required]
    [RegularExpression("^(Sell|Rent|Mortgage)$")]
    public string Property_type {get; set;} = string.Empty;
    [Required]
    [StringLength(200, MinimumLength =4, ErrorMessage ="Description must be between 4, 200 character")]
    public string Description {get; set;} = string.Empty;
    [Required]
    [StringLength(200, MinimumLength =20, ErrorMessage ="Address must be between 20, 200 character")]
    public string Address {get; set;} = string.Empty;
    public List<IFormFile> Photos {get; set;} = new();
}











