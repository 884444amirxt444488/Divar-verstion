


using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

public class Jwt
{
    private readonly IConfiguration configuration;

    public Jwt(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public string GenerateAccessToken(int UserId)
    {
        string Key = configuration["Jwt:Key"] ?? throw new InvalidOperationException("Key not found");
        string Issuer = configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("Issuer not found");
        string Audince = configuration["Jwt:Audince"] ?? throw new InvalidOperationException("Audince not found");
        int ExpiresMinutes = configuration.GetValue<int?>("Jwt:ExpiresMinutes") ?? throw new InvalidOperationException("ExpiresMinutes not found");

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, UserId.ToString())
        };

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key));

        var credentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256
        );

        var AccessToken = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audince,
            expires: DateTime.UtcNow.AddMinutes(ExpiresMinutes),
            claims: claims,
            signingCredentials: credentials
        );
        return new JwtSecurityTokenHandler().WriteToken(AccessToken);
    }
    public string GenerateRefreshToken()
    {
        byte[] RefreshTokenBytes = RandomNumberGenerator.GetBytes(64);

        return Convert.ToBase64String(RefreshTokenBytes);
    }
}











