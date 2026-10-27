using System.Security.Cryptography;
using System.Text;
using Npgsql;
using Shop_project_cs.Dtos.AuthDtos;

public class Auth
{
    private readonly DataBase db;
    private readonly Jwt jwt;
    private readonly EmailServices em;

    public Auth(DataBase db, Jwt jwt, EmailServices em)
    {
        this.db = db;
        this.jwt = jwt;
        this.em = em;
    }

    public async Task<SLDto> Signup(SignupDto NewUser) 
    {
        await using NpgsqlConnection connection = db.CreateDbConnection();
        await connection.OpenAsync();

        await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync();

        try
        {
            await using(NpgsqlCommand command1 = new NpgsqlCommand("SELECT 1 FROM users WHERE username = @username OR email = @email LIMIT 1", connection, transaction))
            {
                command1.Parameters.AddWithValue("username", NewUser.Username);
                command1.Parameters.AddWithValue("email", NewUser.Email);

                Object? UserFound = await command1.ExecuteScalarAsync();

                if (UserFound != null)
                {
                    throw new AppErr(409, "This username or email already exists");
                }
            }

            int UserId;
            string Username;
            string Email;

            await using(NpgsqlCommand command2 = new NpgsqlCommand("INSERT INTO users(username, email, password) VALUES(@username, @email, @password) RETURNING id, username, email", connection, transaction))
            {
                command2.Parameters.AddWithValue("username", NewUser.Username);
                command2.Parameters.AddWithValue("email", NewUser.Email);
                command2.Parameters.AddWithValue("password", BCrypt.Net.BCrypt.HashPassword(NewUser.Password));

                await using(NpgsqlDataReader reader1 = await command2.ExecuteReaderAsync())
                {
                    if (!await reader1.ReadAsync())
                    {
                        throw new AppErr(500, "Can not signup. please try again");
                    }
                    UserId = reader1.GetInt32(0);
                    Username = reader1.GetString(1);
                    Email = reader1.GetString(2);
                }
            }
            
            string AccessToken = jwt.GenerateAccessToken(UserId);
            string RefreshToken = jwt.GenerateRefreshToken();

            await using(NpgsqlCommand command3 = new NpgsqlCommand("INSERT INTO refreshtokens(userid, user_refreshtoken, expires) VALUES(@userid, @user_refreshtoken, @expires)", connection, transaction))
            {
                command3.Parameters.AddWithValue("userid", UserId);
                command3.Parameters.AddWithValue("user_refreshtoken", Convert.ToHexString(SHA512.HashData(Encoding.UTF8.GetBytes(RefreshToken))));
                command3.Parameters.AddWithValue("expires", DateTimeOffset.UtcNow.AddDays(7));

                int IsDbChaged = await command3.ExecuteNonQueryAsync();

                if (IsDbChaged != 1)
                {
                    throw new AppErr(500, "Somthing went wrong. please try again");
                }
            }
            await transaction.CommitAsync();
            return new SLDto
            {
                Username = Username,
                Email = Email,
                AccessToken = AccessToken,
                RefreshToken = RefreshToken
            };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
    public async Task<SLDto> Login(LoginDto User)
    {
        await using NpgsqlConnection connection = db.CreateDbConnection();
        await connection.OpenAsync();

        int UserId;
        string Username;
        string Email;

        await using(NpgsqlCommand command1 = new NpgsqlCommand("SELECT id, username, email, password FROM users WHERE username = @username", connection))
        {
            command1.Parameters.AddWithValue("username", User.Username);

            await using(NpgsqlDataReader reader1 = await command1.ExecuteReaderAsync())
            {
                if (!await reader1.ReadAsync())
                {
                    throw new AppErr(401, "Incorrect username or password");
                }
                if (!BCrypt.Net.BCrypt.Verify(User.Password, reader1.GetString(3)))
                {
                    throw new AppErr(401, "Incorrect username or password");
                }
                UserId = reader1.GetInt32(0);
                Username = reader1.GetString(1);
                Email = reader1.GetString(2);
            }
        }
        
        string AccessToken = jwt.GenerateAccessToken(UserId);
        string RefreshToken = jwt.GenerateRefreshToken();

        await using(NpgsqlCommand command2 = new NpgsqlCommand("INSERT INTO refreshtokens(userid, user_refreshtoken, expires ) VALUES(@userid, @user_refreshtoken, @expires )", connection))
        {
            command2.Parameters.AddWithValue("userid", UserId);
            command2.Parameters.AddWithValue("user_refreshtoken", Convert.ToHexString(SHA512.HashData(Encoding.UTF8.GetBytes(RefreshToken))));
            command2.Parameters.AddWithValue("expires", DateTimeOffset.UtcNow.AddDays(7));

            int IsDbChanged = await command2.ExecuteNonQueryAsync();

            if (IsDbChanged != 1)
            {
                throw new AppErr(500, "Somthing went wrong. please try again");
            }
        }
        return new SLDto
        {
            Username = Username,
            Email = Email,
            AccessToken = AccessToken,
            RefreshToken = RefreshToken
        };
    }
    
    public async Task<PEDto> Profile(int UserId)
    {
        await using NpgsqlConnection connection = db.CreateDbConnection();
        await connection.OpenAsync();

        using NpgsqlCommand command1 = new NpgsqlCommand("SELECT username, email FROM users WHERE id = @userid", connection);

        command1.Parameters.AddWithValue("userid", UserId);
        
        string Username;
        string Email;

        await using(NpgsqlDataReader reader1 = await command1.ExecuteReaderAsync())
        {
            if (!await reader1.ReadAsync())
            {
                throw new AppErr(404, "User not found");
            }
            Username = reader1.GetString(0);
            Email = reader1.GetString(1);
        }

        return new PEDto
        {
            Username = Username,
            Email = Email
        };
    }
    public async Task<bool> DeleteProfile(int UserId)
    {
        await using NpgsqlConnection connection = db.CreateDbConnection();
        await connection.OpenAsync();

        using (NpgsqlCommand command1 = new NpgsqlCommand("DELETE FROM users WHERE id = @userid", connection))
        {
            command1.Parameters.AddWithValue("userid", UserId);

            int IsUserDelete = await command1.ExecuteNonQueryAsync();

            if (IsUserDelete != 1)
            {
                throw new AppErr(404, "User already not exist");
            }
            return true;
        }
    }
    public async Task<PEDto> EditProfile(int UserId, EditDto EditUser) 
    {
        List<string> fields = [];

        await using NpgsqlConnection connection = db.CreateDbConnection();
        await connection.OpenAsync();

        string Username;
        string Email;

        await using(NpgsqlCommand command1 = new NpgsqlCommand("SELECT username, email FROM users WHERE id = @userid", connection))
        {
            command1.Parameters.AddWithValue("userid", UserId);

            await using (NpgsqlDataReader reader1 = await command1.ExecuteReaderAsync())
            {
                if (!await reader1.ReadAsync())
                {
                    throw new AppErr(404, "User not found for edit");
                }
                if (EditUser.Username == reader1.GetString(0) && EditUser.Email == reader1.GetString(1))
                {
                    return new PEDto
                    {
                        Username = reader1.GetString(0),
                        Email = reader1.GetString(1)
                    };
                }
                Username = reader1.GetString(0);
                Email = reader1.GetString(1);
            }
        }

        if (!string.IsNullOrWhiteSpace(EditUser.Username) && EditUser.Username != Username)
        {
            fields.Add("username = @username");
        }
        if (!string.IsNullOrWhiteSpace(EditUser.Email) && EditUser.Email != Email)
        {
            fields.Add("email = @email");
        }

        if (fields.Count == 0)
        {
            throw new AppErr(400, "Username or email is required");
        }

        await using(NpgsqlCommand command1 = new NpgsqlCommand($"SELECT 1 FROM users WHERE ({string.Join(" OR ", fields)}) AND id != @userid LIMIT 1", connection))
        {
            if (!string.IsNullOrWhiteSpace(EditUser.Username))
            {
               if (EditUser.Username != Username)
                {
                    command1.Parameters.AddWithValue("username", EditUser.Username);
                }
            }
            if (!string.IsNullOrWhiteSpace(EditUser.Email))
            {
                if (EditUser.Email != Email)
                {
                    command1.Parameters.AddWithValue("email", EditUser.Email);
                }
            }
            command1.Parameters.AddWithValue("userid", UserId);

            Object? IsUserExist = await command1.ExecuteScalarAsync();

            if (IsUserExist != null)
            {
                throw new AppErr(409, "This username or email already exists");
            }
        }
        await using(NpgsqlCommand command3 = new NpgsqlCommand($"UPDATE users SET {string.Join(", ", fields)} WHERE id = @userid RETURNING username, email", connection))
        {
            if (!string.IsNullOrWhiteSpace(EditUser.Username))
            {
                if (EditUser.Username != Username)
                {
                    command3.Parameters.AddWithValue("username", EditUser.Username);
                }
            }
            if (!string.IsNullOrWhiteSpace(EditUser.Email))
            {
                if (EditUser.Email != Email)
                {
                    command3.Parameters.AddWithValue("email", EditUser.Email);
                }
            }

            command3.Parameters.AddWithValue("userid", UserId);

            if (command3.Parameters.Count == 1)
            {
                return new PEDto
                {
                    Username = Username,
                    Email = Email
                };
            }

            await using(NpgsqlDataReader reader2 = await command3.ExecuteReaderAsync())
            {
                if (!await reader2.ReadAsync())
                {
                    throw new AppErr(500, "Somthing went wrog. please try again");
                }
                Username = reader2.GetString(0);
                Email = reader2.GetString(1); 
            }
        }
        return new PEDto
        {
            Username = Username,
            Email = Email
        };
    }
    public async Task<string> EditPassword(int UserId, EditPassDto EditPass, string RefreshToken)
    {
        if (EditPass.ConfirmPassword != EditPass.NewPassword)
        {
            throw new AppErr(400, "Confirm password is not match with new password");
        }

        await using NpgsqlConnection connection = db.CreateDbConnection();
        await connection.OpenAsync();

        await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync();

        try
        {
            await using(NpgsqlCommand command1 = new NpgsqlCommand("SELECT password FROM users WHERE id = @userid", connection, transaction))
            {
                command1.Parameters.AddWithValue("userid", UserId);

                Object? UserFound = await command1.ExecuteScalarAsync();

                if (UserFound == null)
                {
                    throw new AppErr(404, "User not found for edit");
                }
                if (!BCrypt.Net.BCrypt.Verify(EditPass.OldPassword ,UserFound.ToString()))
                {
                    throw new AppErr(401, "Incorrect password");
                }
                else if (BCrypt.Net.BCrypt.Verify(EditPass.NewPassword, UserFound.ToString()))
                {
                    throw new AppErr(400, "Your password changed successfully");
                }
            }
            await using(NpgsqlCommand command2 = new NpgsqlCommand("UPDATE users SET password = @password WHERE id = @userid", connection, transaction))
            {
                command2.Parameters.AddWithValue("password", BCrypt.Net.BCrypt.HashPassword(EditPass.NewPassword));
                command2.Parameters.AddWithValue("userid", UserId);

                int IsDbChanged = await command2.ExecuteNonQueryAsync();

                if (IsDbChanged != 1)
                {
                    throw new AppErr(500, "Somthing went wrong. please try again");
                }
            }

            string NewRefreshToken = jwt.GenerateRefreshToken();

            await using(NpgsqlCommand command3 = new NpgsqlCommand("UPDATE refreshtokens SET user_refreshtoken = @new_refreshtoken WHERE user_refreshtoken = @user_refreshtoken AND userid = @userid", connection, transaction))
            {
                command3.Parameters.AddWithValue("new_refreshtoken", Convert.ToHexString(SHA512.HashData(Encoding.UTF8.GetBytes(NewRefreshToken))));
                command3.Parameters.AddWithValue("user_refreshtoken", Convert.ToHexString(SHA512.HashData(Encoding.UTF8.GetBytes(RefreshToken))));
                command3.Parameters.AddWithValue("userid", UserId);

                int IsDbChaged = await command3.ExecuteNonQueryAsync();

                if (IsDbChaged != 1)
                {
                    throw new AppErr(500, "Somthing went wrong. please try again");
                }
            }
            await transaction.CommitAsync();
            return NewRefreshToken;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
    public async Task<bool> GetCode(GetCodeDto GC)
    {
        await using NpgsqlConnection connection = db.CreateDbConnection();
        await connection.OpenAsync();

        await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync();

        try 
        {
            await using(NpgsqlCommand command1 = new NpgsqlCommand("SELECT code, expires FROM users WHERE email = @email", connection, transaction))
            {
                command1.Parameters.AddWithValue("email", GC.Email);

                await using(NpgsqlDataReader reader1 = await command1.ExecuteReaderAsync())
                {
                    if (!await reader1.ReadAsync())
                    {
                        throw new AppErr(404, "Incorrect email");
                    }
                    if (!reader1.IsDBNull(1) && DateTimeOffset.UtcNow <= reader1.GetDateTime(1))
                    {
                        throw new AppErr(500, "Your code already valid.");
                    }
                }
            }
            string Code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

            await using(NpgsqlCommand command2 = new NpgsqlCommand("UPDATE users SET code = @code, expires = @expires WHERE email = @email", connection, transaction))
            {
                command2.Parameters.AddWithValue("code", BCrypt.Net.BCrypt.HashPassword(Code));
                command2.Parameters.AddWithValue("expires", DateTimeOffset.UtcNow.AddMinutes(7));
                command2.Parameters.AddWithValue("email", GC.Email);

                int IsDbChanged = await command2.ExecuteNonQueryAsync();

                if (IsDbChanged != 1)
                {
                    throw new AppErr(500, "Somthing went wrong. please try again");
                }
            }
            await em.SendMail(Code, GC.Email);
            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
    public async Task<bool> ChangePassword(ChangeForgottenPassDto CFP) 
    {
        if (CFP.NewPassword != CFP.ConfirmPassword)
        {
            throw new AppErr(400, "New password is not match with confirm password");
        }
        await using NpgsqlConnection connection = db.CreateDbConnection();
        await connection.OpenAsync();

        await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync();

        int UserId;

        try
        {
            await using(NpgsqlCommand command1 = new NpgsqlCommand("SELECT code, expires, id FROM users WHERE email = @email", connection, transaction))
            {
                command1.Parameters.AddWithValue("email", CFP.Email);

                await using(NpgsqlDataReader reader1 = await command1.ExecuteReaderAsync())
                {
                    if (!await reader1.ReadAsync())
                    {
                        throw new AppErr(404, "Incorrect email");
                    }
                    if (reader1.IsDBNull(0) || reader1.IsDBNull(1))
                    {
                        throw new AppErr(400, "First get code");
                    }
                    if (DateTimeOffset.UtcNow > reader1.GetDateTime(1))
                    {
                        throw new AppErr(403, "Code expired");
                    }
                    if (!BCrypt.Net.BCrypt.Verify(CFP.Code, reader1.GetString(0)))
                    {
                        throw new AppErr(403, "Incorrec code");
                    }
                    UserId = reader1.GetInt32(2);
                }
            }
            await using(NpgsqlCommand command2 = new NpgsqlCommand("UPDATE users SET password = @password, code = NULL, expires = NULL WHERE email = @email", connection, transaction))
            {
                command2.Parameters.AddWithValue("password", BCrypt.Net.BCrypt.HashPassword(CFP.NewPassword));
                command2.Parameters.AddWithValue("email", CFP.Email);
                
                int IsDbChaged = await command2.ExecuteNonQueryAsync();

                if (IsDbChaged != 1)
                {
                    throw new AppErr(404, "User not found for change pass");
                }
            }

            string NewRefreshToken = jwt.GenerateRefreshToken();

            await using(NpgsqlCommand command3 = new NpgsqlCommand("UPDATE refreshtokens SET user_refreshtoken = NULL WHERE userid = @userid", connection, transaction))
            {
                command3.Parameters.AddWithValue("userid", UserId);

                int IsDbChaged = await command3.ExecuteNonQueryAsync();

                if (IsDbChaged == 0)
                {
                    throw new AppErr(500, "Somthing went wrong. please try again");
                }
            }

            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
    public async Task<string> RefreshToken(string RefreshToken)
    {
        await using NpgsqlConnection connection = db.CreateDbConnection();
        await connection.OpenAsync();

        string AccessToken;

        await using(NpgsqlCommand command1 = new NpgsqlCommand("SELECT userid FROM refreshtokens WHERE user_refreshtoken = @ref", connection))
        {
            command1.Parameters.AddWithValue("ref", Convert.ToHexString(SHA512.HashData(Encoding.UTF8.GetBytes(RefreshToken))));

            Object? UserId = await command1.ExecuteScalarAsync();

            if (UserId == null)
            {
                throw new AppErr(401, "Not login. login first");
            }
            AccessToken = jwt.GenerateAccessToken(Convert.ToInt32(UserId));
        }
        return AccessToken;
    }
}











