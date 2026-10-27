using Npgsql;


public class DataBase
{
    private readonly string connection;

    public DataBase(IConfiguration configuration)
    {
        connection = configuration.GetConnectionString("DefaultConnectionString") ?? throw new InvalidOperationException("DefaultConnectionString not found");
    }

    public NpgsqlConnection CreateDbConnection()
    {
        return new NpgsqlConnection(connection);
    }
}











