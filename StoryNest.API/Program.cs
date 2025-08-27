using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using StoryNest.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);


/*
 * ************************
 * Database configuration *
 * ************************
 */
// Load .env file
Env.Load();

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


// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
