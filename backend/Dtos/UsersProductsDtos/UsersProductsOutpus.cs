public class UsersProductsOutPutsDto
{
    public decimal Price {get; set;} = 0;
    public string Property_type {get; set;} = "Buy";
    public string Description {get; set;} = string.Empty;
    public string Address {get; set;} = string.Empty;
    public DateTime Created_at {get; set;} = DateTime.UtcNow;
    public string Photo_address {get; set;} = string.Empty;

}