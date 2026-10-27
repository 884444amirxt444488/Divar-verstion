
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Shop_project_cs.Dtos.AuthDtos;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly Auth auth;

    public AuthController(Auth auth)
    {
        this.auth = auth;
    }

    [EnableRateLimiting("LoginLimiter")]
    [HttpPost("Signup")]
    public async Task<IActionResult> Signup([FromBody] SignupDto NewUser)
    {
        try
        {
            SLDto Result = await auth.Signup(NewUser);
            Response.Cookies.Append(
                "RefreshToken",
                Result.RefreshToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddDays(7)
                }
            );
            return Created("api/Auth/Signup", new
            {
                Message = "Signup successfully",
                Username = Result.Username,
                Email = Result.Email,
                AccessToken = Result.AccessToken
            });
        }
        catch (AppErr ApE)
        {
            return StatusCode(ApE.StatusCode, ApE.Message);
        }
        catch (Exception Err)
        {
            Console.WriteLine($"Error to singunp: {Err}");
            return StatusCode(500, "Error from server. please try later");
        }
    }
    [EnableRateLimiting("LoginLimiter")]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto User)
    {
        try
        {
            SLDto Result = await auth.Login(User);
            Response.Cookies.Append(
                "RefreshToken",
                Result.RefreshToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddDays(7)
                }
            );
            return Ok(new
            {
                Message = "Login successfully",
                Username = Result.Username,
                Email = Result.Email,
                AccessToken = Result.AccessToken
            });
        }
        catch (AppErr ApE)
        {
            return StatusCode(ApE.StatusCode, ApE.Message);
        }
        catch (Exception Err)
        {
            Console.WriteLine($"Error to login: {Err}");
            return StatusCode(500, "Error from server. please try later");
        }
    }
    [Authorize]
    [HttpGet("Profile")]
    public async Task<IActionResult> Profile()
    {
        if (!int.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out int UserId))
        {
            return Unauthorized("Not login. login first");
        }
        try
        {
            PEDto Result = await auth.Profile(UserId);
            return Ok(new
            {
                Message = "****WELCOM****",
                Username = Result.Username,
                Email = Result.Email
            });
        }
        catch (AppErr ApE)
        {
            return StatusCode(ApE.StatusCode, ApE.Message);
        }
        catch (Exception Err)
        {
            Console.WriteLine($"Error to get profile: {Err}");
            return StatusCode(500, "Error from server. please try later");
        }
    }
    [Authorize]
    [HttpDelete("DeleteProfile")]
    public async Task<IActionResult> DeleteUser()
    {
        if (!int.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out int UserId))
        {
            return Unauthorized("Not login. login first");
        }
        try
        {
            await auth.DeleteProfile(UserId);
            Response.Cookies.Delete("RefreshToken", new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Strict
            });
            return Ok(new
            {
                Message = "User deleted successfully"
            });
        }   
        catch (AppErr ApE)
        {
            return StatusCode(ApE.StatusCode, ApE.Message);
        }
        catch (Exception Err)
        {
            Console.WriteLine($"Error to delete user: {Err}");
            return StatusCode(500, "Error from server. please try later");
        }
    }
    [Authorize]
    [EnableRateLimiting("LoginLimiter")]
    [HttpPatch("EditUser")]
    public async Task<IActionResult> EditUser([FromBody] EditDto EditUser)
    {
        if (!int.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out int UserId))
        {
            return Unauthorized("Not login. login first");
        }
        try
        {
            PEDto Result = await auth.EditProfile(UserId, EditUser);
            return Ok(new
            {
                Message = "User updated successfully",
                Username = Result.Username,
                Email = Result.Email
            });
        }   
        catch (AppErr ApE)
        {
            return StatusCode(ApE.StatusCode, ApE.Message);
        }
        catch (Exception Err)
        {
            Console.WriteLine($"Error to delete user: {Err}");
            return StatusCode(500, "Error from server. please try later");
        }
    }
    [Authorize]
    [EnableRateLimiting("LoginLimiter")]
    [HttpPost("EditPassword")]
    public async Task<IActionResult> EditPassword([FromBody] EditPassDto EditPass)
    {
        string? RefreshToken = Request.Cookies["RefreshToken"];

        if (RefreshToken == null)
        {
            return Unauthorized("Not login. login first");
        }

        if (!int.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out int UserId))
        {
            return Unauthorized("Not login. login first");
        }
        try
        {
            string Result = await auth.EditPassword(UserId, EditPass, RefreshToken);
            Response.Cookies.Append(
                "RefreshToken",
                Result,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddDays(7)
                }
            );
            return Ok(new
            {
                Message = "Password updated successfully"
            });
        }   
        catch (AppErr ApE)
        {
            return StatusCode(ApE.StatusCode, ApE.Message);
        }
        catch (Exception Err)
        {
            Console.WriteLine($"Error to delete user: {Err}");
            return StatusCode(500, "Error from server. please try later");
        }
    }
    [EnableRateLimiting("LoginLimiter")]
    [HttpPost("GetCode")]
    public async Task<IActionResult> GetCode([FromBody] GetCodeDto GC)
    {
        try
        {
            await auth.GetCode(GC);
            return Ok(new
            {
                Message = "Code sended successfully"
            });
        }   
        catch (AppErr ApE)
        {
            return StatusCode(ApE.StatusCode, ApE.Message);
        }
        catch (Exception Err)
        {
            Console.WriteLine($"Error to get code: {Err}");
            return StatusCode(500, "Error from server. please try later");
        }
    }
    [HttpPost("ChangeForgottenPassword")]
    public async Task<IActionResult> ChangeForgottenPassword([FromBody] ChangeForgottenPassDto CFP)
    {
        try
        {
            await auth.ChangePassword(CFP);
            return Ok(new
            {
                Message = "Password changed successfully"
            });
        }   
        catch (AppErr ApE)
        {
            return StatusCode(ApE.StatusCode, ApE.Message);
        }
        catch (Exception Err)
        {
            Console.WriteLine($"Error to change forgotten password: {Err}");
            return StatusCode(500, "Error from server. please try later");
        }
    }
    [HttpPost("RefreshToken")]
    public async Task<IActionResult> RefreshToken()
    {
        string? RefreshToken = Request.Cookies["RefreshToken"];
        if (RefreshToken == null)
        {
            return Unauthorized("Login first");
        }
        try
        {
            string Result = await auth.RefreshToken(RefreshToken);
            return Ok(new
            {
                AccessToken = Result
            });
        }
        catch (AppErr ApE)
        {
            return StatusCode(ApE.StatusCode, ApE.Message);
        }
        catch (Exception Err)
        {
            Console.WriteLine($"Error to refresh token: {Err}");
            return StatusCode(500, "Error from server. please try later");
        }
    }
}







