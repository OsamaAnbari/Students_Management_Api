using Students_Management_Api;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Students_Management_Api.Middlewares;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Students_Management_Api.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;


var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;
var configuration = builder.Configuration;


services.AddEndpointsApiExplorer();
services.AddSwaggerGen(option =>
{
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configuration["key"])),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});
services.AddAuthorization();

services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

services.AddDbContext<LibraryContext>(x => x.UseMySQL(configuration["ConnectionStrings:sql"]));
services.AddIdentity<ApplicationUser, IdentityRole>()
        .AddEntityFrameworkStores<LibraryContext>()
        .AddDefaultTokenProviders();
services.Configure<IdentityOptions>(options =>
{
    options.ClaimsIdentity.UserNameClaimType = ClaimTypes.Name; // Change the default username claim type
    options.ClaimsIdentity.UserIdClaimType = ClaimTypes.NameIdentifier; // Change the default user ID claim type
    options.ClaimsIdentity.RoleClaimType = ClaimTypes.Role; // Change the default role claim type
});

services.AddStackExchangeRedisCache(options => { options.Configuration = configuration["RedisCacheUrl"]; });


// Configure the HTTP request pipeline.

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

//app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<GetIdRole>();

app.MapControllers();

app.Run();
