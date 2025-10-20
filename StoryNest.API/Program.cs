using DotNetEnv;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;
using OpenAI.Audio;
using OpenAI.Images;
using Quartz;
using Resend;
using StackExchange.Redis;
using StoryNest.API.Hubs;
using StoryNest.API.Services;
using StoryNest.Application.Dtos.Request;
using StoryNest.Application.Dtos.Validator;
using StoryNest.Application.Features.Users;
using StoryNest.Application.Interfaces;
using StoryNest.Application.Mappings;
using StoryNest.Application.Services;
using StoryNest.Domain.Interfaces;
using StoryNest.Infrastructure.Persistence;
using StoryNest.Infrastructure.Persistence.Repositories;
using StoryNest.Infrastructure.Services;
using StoryNest.Infrastructure.Services.Email;
using StoryNest.Infrastructure.Services.Google;
using StoryNest.Infrastructure.Services.Google.GoogleNLP;
using StoryNest.Infrastructure.Services.LogoProvider;
using StoryNest.Infrastructure.Services.OpenAI;
using StoryNest.Infrastructure.Services.PayOSPayment;
using StoryNest.Infrastructure.Services.QuartzSchedule;
using StoryNest.Infrastructure.Services.QuestPdfService;
using StoryNest.Infrastructure.Services.Redis;
using StoryNest.Infrastructure.Services.S3;
using StoryNest.Infrastructure.Services.VnCoreNlp;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins("https://localhost:3000", "http://localhost:3000", "https://storynest-fe.kusl.io.vn", "https://dev.storynest.io.vn", "https://storynest.io.vn", "http://127.0.0.1:5000")
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
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            if (!string.IsNullOrEmpty(accessToken) &&
            path.StartsWithSegments("/hubs/notify"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "StoryNest API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập token theo dạng: Bearer {token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });

    c.UseInlineDefinitionsForEnums();
});

// Fluent Validation
builder.Services.AddScoped<IValidator<RegisterUserRequest>, RegisterUserRequestValidator>();
builder.Services.AddScoped<IValidator<LoginUserRequest>, LoginUserRequestValidator>();
builder.Services.AddScoped<IValidator<CreateStoryRequest>, CreateStoryRequestValidator>();
builder.Services.AddScoped<IValidator<UploadMediaRequest>, UploadImageRequestValidator>();
builder.Services.AddScoped<IValidator<LoginAdminRequest>, LoginAdminRequestValidator>();

// Repositories 
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IStoryRepository, StoryRepository>();
builder.Services.AddScoped<ITagRepository, TagRepository>();
builder.Services.AddScoped<IStoryTagRepository, StoryTagRepository>();
builder.Services.AddScoped<IMediaRepository, MediaRepository>();
builder.Services.AddScoped<ILikeRepository, LikeRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<IAICreditRepository, AICreditRepository>();
builder.Services.AddScoped<IUserMediaRepository, UserMediaRepository>();
builder.Services.AddScoped<IAITransactionRepository, AITransactionRepository>();    
builder.Services.AddScoped<IAIUsageLogRepository, AIUsageLogRepository>();
builder.Services.AddScoped<IPlanRepository, PlanRepository>();
builder.Services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IStoryViewRepository, StoryViewRepository>();
builder.Services.AddScoped<IUserReportRepository, UserReportRepository>();
builder.Services.AddScoped<IAdminRepository, AdminRepository>();
builder.Services.AddScoped<IStorySentimentAnalysisRepository, StorySentimentAnalysisRepository>();

//Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IStoryService, StoryService>();
builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddScoped<IStoryTagService, StoryTagService>();
builder.Services.AddScoped<IMediaService, MediaService>();
builder.Services.AddScoped<ILikeService, LikeService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IGoogleService, GoogleService>();
builder.Services.AddScoped<IAICreditService, AICreditService>();
builder.Services.AddScoped<IUserMediaService, UserMediaService>();
builder.Services.AddScoped<IAITransactionService, AITransactionService>();
builder.Services.AddScoped<IAIUsageLogService, AIUsageLogService>();
builder.Services.AddScoped<IPlanService, PlanService>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IPayOSPaymentService, PayOSPaymentService>();
builder.Services.AddScoped<INotificationHubService, NotificationHubService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IStoryViewService, StoryViewService>();
builder.Services.AddScoped<IVnCoreNlpService, VnCoreNlpService>();
builder.Services.AddScoped<IUserReportService, UserReportService>();
builder.Services.AddScoped<IGoogleNLPService, GoogleNLPService>();
builder.Services.AddScoped<IStorySentimentAnalysisService, StorySentimentAnalysisService>();

// Email Services
builder.Services.AddScoped<ITemplateRenderer, TemplateEmailRenderer>();
builder.Services.AddScoped<WelcomeEmailSender>();
builder.Services.AddScoped<WelcomeEmailGoogleSender>();
builder.Services.AddScoped<ResetPasswordEmailSender>();
builder.Services.AddScoped<InvoiceEmailSender>();

// Others
builder.Services.AddScoped<IRedisService, RedisService>();
builder.Services.AddScoped<IS3Service, S3Service>();
builder.Services.AddScoped<IUploadService, UploadService>();
builder.Services.AddScoped<IOpenAIService, OpenAIService>();    
builder.Services.AddAutoMapper(typeof(StoryProfile));
builder.Services.AddAutoMapper(typeof(UserProfile));
builder.Services.AddSingleton<ILogoProvider, FileLogoProvider>();
builder.Services.AddScoped<IQuestPdfService, QuestPdfService>();
//builder.Services.AddScoped<RenewCreditJob>();

builder.Services.AddQuartz(q =>
{
    var renewCreditJobKey = new JobKey(nameof(RenewCreditJob));
    q.AddJob<RenewCreditJob>(opts => opts.WithIdentity(renewCreditJobKey));
    q.AddTrigger(opts => opts
        .ForJob(renewCreditJobKey)
        .WithIdentity($"{nameof(RenewCreditJob)}-trigger")
        .WithCronSchedule("0 0 0 * * ?", x => x.InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh"))));

    var storySentimentAnalysisJobKey = new JobKey(nameof(CheckReportStoryJob));
    q.AddJob<CheckReportStoryJob>(opts => opts.WithIdentity(storySentimentAnalysisJobKey));
    q.AddTrigger(opts => opts
        .ForJob(storySentimentAnalysisJobKey)
        .WithIdentity($"{nameof(CheckReportStoryJob)}-trigger")
        //.WithCronSchedule("0 0 7,13,20 * * ?", x => x.InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh"))));
    .WithCronSchedule("0 0/3 * * * ?", x => x.InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh"))));
});

builder.Services.AddQuartzHostedService(opt =>
{
    opt.WaitForJobsToComplete = true;
});

builder.Services.AddSingleton<ImageClient>(serviceProvider =>
{
    var apiKey = builder.Configuration["OPENAI_API_KEY"];
    var model = "dall-e-3";
    return new ImageClient(model, apiKey);
});

builder.Services.AddSingleton<AudioClient>(serviceProvider =>
{
    var apiKey = builder.Configuration["OPENAI_API_KEY"];
    var model = "gpt-4o-mini-tts";
    return new AudioClient(model, apiKey);
});

builder.Services.AddSignalR();

var app = builder.Build();
app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notify");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
    db.Database.Migrate();
    StoryNest.Infrastructure.Persistence.Seed.ApplicationDbContextSeed.SeedAdmins(db, builder.Configuration);
}

app.Run();
