using Npgsql;
using Shop_project_cs.Dtos.UsersProductsDtos;


public class Users_Products
{
    private readonly DataBase db;

    public Users_Products(DataBase db)
    {
        this.db = db;
    }

    public async Task<UsersProductsOutPutsDto> Add_User_Product(AddUserProductDto AddUP) 
    {
        await using NpgsqlConnection connection = db.CreateDbConnection();
        await connection.OpenAsync();

        await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync();

        decimal Price;
        string Property_type;
        string Description;
        string Address;

        try
        {
            await using(NpgsqlCommand command1 = new NpgsqlCommand("INSERT INTO users_products(userid, price, property_type, description, address) VALUES(@userid, @price, @property_type, @description, @address) RETURNING price, property_type, description, address", connection, transaction))
            {
                command1.Parameters.AddWithValue("userid", AddUP.UserId);
                command1.Parameters.AddWithValue("price", AddUP.Price);
                command1.Parameters.AddWithValue("property_type", AddUP.Property_type);
                command1.Parameters.AddWithValue("description", AddUP.Description);
                command1.Parameters.AddWithValue("address", AddUP.Address);

                await using(NpgsqlDataReader reader1 = await command1.ExecuteReaderAsync())
                {
                    if (!await reader1.ReadAsync())
                    {
                        throw new AppErr(500, "Somthing went wrong. please try again");
                    }
                    Price = reader1.GetDecimal(0);
                    Property_type = reader1.GetString(1);
                    Description = reader1.GetString(2);
                    Address = reader1.GetString(3);
                }
            }




        }



    }






}



















