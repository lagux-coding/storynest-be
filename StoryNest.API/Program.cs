using DotNetEnv;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StoryNest.Application.Dtos.Request;
using StoryNest.Application.Dtos.Validator;
using StoryNest.Application.Interfaces;
using StoryNest.Application.Services;
using StoryNest.Domain.Interfaces;
using StoryNest.Infrastructure.Persistence;
using StoryNest.Infrastructure.Persistence.Repositories;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


/*
 * ************************
 * Database configuration *
 * ************************
 */
// Load .env file
Env.Load();
foreach (System.Collections.DictionaryEntry de in Environment.GetEnvironmentVariables())
{
    var key = de.Key?.ToString();
    var value = de.Value?.ToString();

    if (key != null && value != null)
    {
        builder.Configuration[key] = value;
    }
}


// Get the connection string from environment variables
var host = Environment.GetEnvironmentVariable("DB_HOST");
var port = Environment.GetEnvironmentVariable("DB_PORT");
var database = Environment.GetEnvironmentVariable("DB_NAME");
var username = Environment.GetEnvironmentVariable("DB_USER");
var password = Environment.GetEnvironmentVariable("DB_PASSWORD");

// Build the connection string
var connectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password}";

// Add DbContext
builder.Services.AddDbContext<MyDbContext>(options => options.UseNpgsql(connectionString));
// ************************************************

// Jwt
builder.Services.AddAuthentication(options =>
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
        ValidIssuer = builder.Configuration["JWT_ISSUER"],
        ValidAudience = builder.Configuration["JWT_AUDIENCE"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT_KEY"])),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
    };
});

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Fluent Validation
builder.Services.AddScoped<IValidator<RegisterUserRequest>, RegisterUserRequestValidator>();
builder.Services.AddScoped<IValidator<LoginUserRequest>, LoginUserRequestValidator>();

// Repositories 
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

//Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtService, JwtService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();


app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
