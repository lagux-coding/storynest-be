using DotNetEnv;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Resend;
using StackExchange.Redis;
using StoryNest.Application.Dtos.Request;
using StoryNest.Application.Dtos.Validator;
using StoryNest.Application.Features.Users;
using StoryNest.Application.Interfaces;
using StoryNest.Application.Services;
using StoryNest.Domain.Interfaces;
using StoryNest.Infrastructure.Persistence;
using StoryNest.Infrastructure.Persistence.Repositories;
using StoryNest.Infrastructure.Services.Email;
using StoryNest.Infrastructure.Services.Redis;
using StoryNest.Infrastructure.Services.User;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins("https://localhost:3000", "http://localhost:3000")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});


builder.Services.AddHttpContextAccessor();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

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

// Redis DI
var redisHost = builder.Configuration["Redis:Host"] ?? "localhost";
var redisPort = builder.Configuration["Redis:Port"] ?? "6379";
var redisPassword = builder.Configuration["REDIS_PASSWORD"];
var redisConnection = builder.Configuration["REDIS_CONNECTION_STRING"];
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    if (redisHost != "localhost" || redisPassword != null)
    {
        redisConnection = redisConnection = $"{redisHost}:{redisPort},password={redisPassword},abortConnect=false";
    }
    return ConnectionMultiplexer.Connect(redisConnection);
});

// Resend DI
builder.Services.AddOptions();
builder.Services.AddHttpClient<ResendClient>();
builder.Services.Configure<ResendClientOptions>(options =>
{
    options.ApiToken = builder.Configuration["RESEND_API_KEY"]
                        ?? Environment.GetEnvironmentVariable("RESEND_API_KEY");
});
builder.Services.AddTransient<IResend, ResendClient>();
builder.Services.AddEmail(builder.Configuration);

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
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Email Services
builder.Services.AddScoped<ITemplateRenderer, TemplateEmailRenderer>();
builder.Services.AddScoped<WelcomeEmailSender>();
builder.Services.AddScoped<ResetPasswordEmailSender>();

// Others
builder.Services.AddScoped<IRedisService, RedisService>();

var app = builder.Build();
app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();


app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
