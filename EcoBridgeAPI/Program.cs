using EcoBridge.API.Middleware;
using EcoBridge.Application.BackgroundJobs;
using EcoBridge.Application.Contracts;
using EcoBridge.Application.Extensions;
using EcoBridge.Application.Features.Notifications;
using EcoBridge.Application.Hubs;
using EcoBridge.Application.Result;
using EcoBridge.Application.Services;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Models;
using EcoBridge.Domain.Models.Accounts;
using EcoBridge.Infrastructure.BackgroundJobs;
using EcoBridge.Infrastructure.Binders;
using EcoBridge.Infrastructure.Data;
using EcoBridge.Infrastructure.Services.Caching;
using EcoBridge.Infrastructure.Services.Implementation;
using EcoBridge.Infrastructure.Services.Implementation.Cloudinary;
using EcoBridge.Infrastructure.Services.Photo;
using FluentValidation;
using Hangfire;
using Hangfire.SqlServer;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Globalization;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// =========================
// Services
// =========================

builder.Services.AddControllers(options =>
{
    options.ModelBinderProviders.Insert(0, new EnumStringModelBinderProvider());
}).AddJsonOptions(option =>
{
    option.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(namingPolicy:null,allowIntegerValues:false));
    option.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    
});
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(e => e.Value?.Errors.Count > 0)
            .SelectMany(x => x.Value!.Errors)
            .Select(x => x.ErrorMessage != string.Empty ? x.ErrorMessage : x.Exception?.Message)
            .ToList();

        var errorMessage = string.Join(" | ", errors);

        var result = ApiResult<string>.Fail(AppMessageCodes.VALIDATION_ERROR, errorMessage);
        return new BadRequestObjectResult(result);
    };
});

// Domain & Feature Services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IPhotoService, PhotoService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// New Services (Cloudinary, Email, Cookie, CurrentUser, Enhanced Redis)
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ICookieService, CookieService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IRedisServices, RedisServices>();

// Real-Time Hubs (PRD 3)
builder.Services.AddSignalR();
// PRD 2 Services (Auth, Caching, Geographic)
builder.Services.AddScoped<IRefreshTokenService, RedisRefreshTokenService>();
builder.Services.AddScoped<ICacheService, RedisCacheService>();
builder.Services.AddScoped<IOutboxService, OutboxService>();
builder.Services.AddScoped<DonationExpiryJob>();
builder.Services.AddScoped<TransientDataCleanupJob>();
builder.Services.AddScoped<VolunteerTimeoutJob>();
builder.Services.AddScoped<OutboxProcessorJob>();
builder.Services.AddHostedService<OutboxProcessorHostedService>();

builder.Services.AddDbContext<EcoBridgeDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IEcoBridgeDbContext>(sp => sp.GetRequiredService<EcoBridgeDbContext>());
//builder.Services.AddScoped<LocationDataSeeder>();

// =========================
// ASP.NET Core Identity (Selective Schema)
// =========================
builder.Services.AddIdentityCore<Account>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
})
.AddRoles<ApplicationRole>()
.AddEntityFrameworkStores<EcoBridgeDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddScoped<IPasswordHasher<Account>, PasswordHasher<Account>>();

// =========================
// Background Jobs (Hangfire)
// =========================
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"), new SqlServerStorageOptions
    {
        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
        QueuePollInterval = TimeSpan.Zero,
        UseRecommendedIsolationLevel = true,
        DisableGlobalLocks = true
    }));

builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = Math.Min(Environment.ProcessorCount * 2, 8);
});

// =========================
// Caching & MediatR (PRD 2)
// =========================
builder.Services.AddMemoryCache();

var redisConn = builder.Configuration["Redis:ConnectionString"] 
                ?? builder.Configuration.GetConnectionString("Redis") 
                ?? "localhost:6379,abortConnect=false";
var redisPrefix = builder.Configuration["Redis:InstancePrefix"] ?? "EcoBridge:";

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConn;
    options.InstanceName = redisPrefix;
});

try
{
    var multiplexer = ConnectionMultiplexer.Connect(redisConn);
    builder.Services.AddSingleton<IConnectionMultiplexer>(multiplexer);
}
catch (Exception ex)
{
    Console.WriteLine($"Could not connect to Redis multiplexer: {ex.Message}");
}

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(TokenService).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});
builder.Services.AddValidatorsFromAssembly(typeof(TokenService).Assembly);



builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingPipelineBehavior<,>));

builder.Services.Configure<JWTSettings>(builder.Configuration.GetSection("JWT"));
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("Cloudinary"));

// Validate Cloudinary configuration and log guidance
var cloudinarySettings = builder.Configuration.GetSection("Cloudinary").Get<CloudinarySettings>();
if (cloudinarySettings == null ||
    string.IsNullOrWhiteSpace(cloudinarySettings.CloudName) ||
    string.IsNullOrWhiteSpace(cloudinarySettings.ApiKey) ||
    string.IsNullOrWhiteSpace(cloudinarySettings.ApiSecret))
{
    Console.WriteLine("Cloudinary is not fully configured. Photo uploads will be disabled. To enable, set Cloudinary:CloudName, Cloudinary:ApiKey and Cloudinary:ApiSecret in configuration.");
}

// =========================
// Swagger & OpenAPI
// =========================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi("v1");
builder.Services.AddSwaggerGen();

// =========================
// JWT Authentication
// =========================
var jwtSettings = builder.Configuration.GetSection("JWT").Get<JWTSettings>();

if (jwtSettings == null || string.IsNullOrWhiteSpace(jwtSettings.SecretKey) ||
    string.IsNullOrWhiteSpace(jwtSettings.Issuer) || string.IsNullOrWhiteSpace(jwtSettings.Audience))
{
    throw new InvalidOperationException("JWT configuration is missing or incomplete. Please configure 'JWT' section in appsettings.");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
    };

    options.Events = new JwtBearerEvents
    {
        OnChallenge = async context =>
        {
            context.HandleResponse();
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            var response = ApiResult.Fail(AppMessageCodes .UNAUTHORIZED);
            await context.Response.WriteAsJsonAsync(response);
        },
        OnForbidden = async context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";
            var response = ApiResult.Fail(AppMessageCodes .FORBIDDEN);
            await context.Response.WriteAsJsonAsync(response);
        }
    };
});

// =========================
// CORS
// =========================
var allowedOrigin = builder.Configuration.GetValue<string>("AllowOrigins");

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        if (allowedOrigin == "*")
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
        else if (!string.IsNullOrEmpty(allowedOrigin))
        {
            policy.WithOrigins(allowedOrigin)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        }
    });
});

// =========================
// Rate Limiting (PRD 2)
// =========================
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, token) =>
    {
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter =
                ((int)retryAfter.TotalSeconds).ToString(CultureInfo.InvariantCulture);
        }
        context.HttpContext.Response.ContentType = "application/json";
        var errorResponse = ApiResult.Fail(AppMessageCodes.TOO_MANY_REQUESTS);
        await context.HttpContext.Response.WriteAsJsonAsync(errorResponse, cancellationToken: token);
    };

    // 1. Fixed Window Limiter (Security Focus: Login, Register, OTP) - Max 5 requests / 1 min partitioned by IP / UserId
    options.AddPolicy("SecurityFixedWindow", httpContext =>
    {
        var partitionKey = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
                           ?? httpContext.Connection.RemoteIpAddress?.ToString()
                           ?? "Unknown";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: partitionKey,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            });
    });

    // 2. Token Bucket Limiter (UX Focus: General Read/Write APIs) - 20 capacity, refills 2 requests/sec
    options.AddPolicy("GeneralTokenBucket", httpContext =>
    {
        var partitionKey = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
                           ?? httpContext.Connection.RemoteIpAddress?.ToString()
                           ?? "Unknown";

        return RateLimitPartition.GetTokenBucketLimiter(
            partitionKey: partitionKey,
            factory: _ => new TokenBucketRateLimiterOptions
            {
                AutoReplenishment = true,
                TokenLimit = 20,
                TokensPerPeriod = 2,
                ReplenishmentPeriod = TimeSpan.FromSeconds(1),
                QueueLimit = 5
            });
    });

    // 3. Concurrency Limiter (CPU/Hashing Focus: Auth Hashing) - Max 10 concurrent requests
    options.AddPolicy("ConcurrencyAuth", _ =>
        RateLimitPartition.GetConcurrencyLimiter(
            partitionKey: "AuthConcurrencyGroup",
            factory: _ => new ConcurrencyLimiterOptions
            {
                PermitLimit = 10,
                QueueLimit = 20,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            }));

    // 4. Concurrency Limiter (Uploads Focus: Photo/File uploads) - Max 5 concurrent uploads
    options.AddPolicy("ConcurrencyUpload", _ =>
        RateLimitPartition.GetConcurrencyLimiter(
            partitionKey: "UploadConcurrencyGroup",
            factory: _ => new ConcurrencyLimiterOptions
            {
                PermitLimit = 5,
                QueueLimit = 10,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            }));

    // Legacy policies maintained for compatibility
    options.AddPolicy("IpRateLimit", httpContext =>
        RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
            factory: _ => new SlidingWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 6
            }));

    options.AddPolicy("UserRateLimit", httpContext =>
    {
        var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Anonymous";
        return RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: userId,
            factory: _ => new SlidingWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 40,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 6
            });
    });
});

// =========================
// Build App & Middleware Pipeline
// =========================
var app = builder.Build();
app.UseHttpsRedirection();

app.UseMiddleware<GlobalExceptionMiddleware>();

app.MapOpenApi();

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "EcoBridge API v1");
    options.RoutePrefix = "swagger";
});

//app.UseHttpsRedirection();
app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.UseRateLimiter();
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireDashboardAuthorizationFilter() }
});

// Recurring Background Jobs (PRD 3 & Data Retention)
RecurringJob.AddOrUpdate<DonationExpiryJob>("donation-expiry-monitor", job => job.ExecuteAsync(), Cron.Hourly);
RecurringJob.AddOrUpdate<VolunteerTimeoutJob>("volunteer-timeout-monitor", job => job.ExecuteAsync(), Cron.Hourly);
RecurringJob.AddOrUpdate<TransientDataCleanupJob>("transient-data-cleanup", job => job.ExecuteAsync(CancellationToken.None), Cron.Daily);
RecurringJob.AddOrUpdate<OutboxProcessorJob>("outbox-processor", job => job.ProcessPendingMessagesAsync(CancellationToken.None), "*/1 * * * *");

app.MapControllers();
app.MapHub<MissionHub>("/hubs/missions");

//// Seed Location Data (Egypt, 27 Governorates, up to 20 Cities each)
//using (var scope = app.Services.CreateScope())
//{
//    var seeder = scope.ServiceProvider.GetRequiredService<LocationDataSeeder>();
//    await seeder.SeedAsync();
//}

app.Run();

public partial class Program { }
