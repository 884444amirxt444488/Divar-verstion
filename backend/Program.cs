/*
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options =>
{
    options.ModelBinderProviders.Insert(
        0,
        new TrimAllModelBinder()
    );
}).AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new TrimJsonModel());
});
builder.Services.AddScoped<DataBase>();
builder.Services.AddScoped<Jwt>();
builder.Services.AddScoped<Auth>();
builder.Services.AddScoped<EmailServices>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.MapInboundClaims = false;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],

        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audince"],

        ValidateLifetime = true,

        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),

        ClockSkew = TimeSpan.Zero
    };
});
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddFixedWindowLimiter("LoginLimiter", limiter =>
    {
        limiter.PermitLimit = 5;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueLimit = 0;
        limiter.AutoReplenishment = true;
    });
});


var App = builder.Build();

App.UseRouting();
App.UseRateLimiter();

App.UseAuthentication();
App.UseAuthorization();

App.MapControllers();

App.Run();
*/

using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options =>
{
    options.ModelBinderProviders.Insert(
        0, 
        new TrimAllModelBinder()
    );
}).AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(
        new TrimJsonModel()
    );
});
builder.Services.AddScoped<DataBase>();
builder.Services.AddScoped<EmailServices>();
builder.Services.AddScoped<Jwt>();
builder.Services.AddScoped<Auth>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.MapInboundClaims = false;

    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],

        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audince"],

        ValidateLifetime = true,

        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),

        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddFixedWindowLimiter("LoginLimiter", limiter =>
    {
        limiter.PermitLimit = 8;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueLimit = 0;
        limiter.AutoReplenishment = true;
    });
});

var App = builder.Build();

App.UseRouting();
App.UseRateLimiter();

App.UseAuthentication();
App.UseAuthorization();

App.MapControllers();

App.Run();








