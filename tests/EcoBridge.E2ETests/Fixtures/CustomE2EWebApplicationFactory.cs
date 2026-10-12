using System.Collections.Generic;
using System.Threading.Tasks;
using EcoBridge.Application.Contracts;
using EcoBridge.Application.Services;
using EcoBridge.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using StackExchange.Redis;
using Xunit;

namespace EcoBridge.E2ETests.Fixtures;

public class CustomE2EWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly EcoBridgeE2EContainersFixture _containersFixture;

    public Mock<IEmailService> EmailServiceMock { get; } = new();
    public Mock<ICloudinaryService> CloudinaryServiceMock { get; } = new();

    public CustomE2EWebApplicationFactory()
    {
        _containersFixture = new EcoBridgeE2EContainersFixture();
    }

    public async Task InitializeAsync()
    {
        await _containersFixture.InitializeAsync();

        // Migrate DB schema
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EcoBridgeDbContext>();
        await db.Database.MigrateAsync();
        await SeedRequiredDataAsync(db);
    }

    private static async Task SeedRequiredDataAsync(EcoBridgeDbContext db)
    {
        if (!await db.Categories.AnyAsync())
        {
            var categories = new[]
            {
                EcoBridge.Domain.Models.Category.Create("طعام جاهز", "Prepared Meals"),
                EcoBridge.Domain.Models.Category.Create("مخبوزات", "Bakery"),
                EcoBridge.Domain.Models.Category.Create("خضروات وفواكه", "Produce"),
                EcoBridge.Domain.Models.Category.Create("معلبات", "Canned Goods"),
                EcoBridge.Domain.Models.Category.Create("أخرى", "Other")
            };
            db.Categories.AddRange(categories);
            await db.SaveChangesAsync();
        }

        if (!await db.ReviewReasons.AnyAsync())
        {
            var reasons = new[]
            {
                EcoBridge.Domain.Models.ReviewReason.Create("PUNCTUAL", EcoBridge.Domain.Enums.UserRole.Donor, EcoBridge.Domain.Enums.UserRole.Volunteer, true),
                EcoBridge.Domain.Models.ReviewReason.Create("POLITE", EcoBridge.Domain.Enums.UserRole.Donor, EcoBridge.Domain.Enums.UserRole.Volunteer, true),
                EcoBridge.Domain.Models.ReviewReason.Create("LATE", EcoBridge.Domain.Enums.UserRole.Donor, EcoBridge.Domain.Enums.UserRole.Volunteer, false),
                EcoBridge.Domain.Models.ReviewReason.Create("DAMAGED", EcoBridge.Domain.Enums.UserRole.Charity, EcoBridge.Domain.Enums.UserRole.Volunteer, false),
                EcoBridge.Domain.Models.ReviewReason.Create("ACCURATE", EcoBridge.Domain.Enums.UserRole.Charity, EcoBridge.Domain.Enums.UserRole.Donor, true)
            };
            db.ReviewReasons.AddRange(reasons);
            await db.SaveChangesAsync();
        }
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await _containersFixture.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            var testConfig = new Dictionary<string, string?>
            {
                { "ConnectionStrings:DefaultConnection", _containersFixture.GetSqlConnectionString() },
                { "Redis:ConnectionString", _containersFixture.GetRedisConnectionString() },
                { "Redis:InstancePrefix", "EcoBridgeE2E:" },
                { "JWT:SecretKey", "TestSuperSecretKeyForE2ETesting20261234567890!" },
                { "JWT:Issuer", "EcoBridgeAPI" },
                { "JWT:Audience", "EcoBridgeClients" },
                { "Cloudinary:CloudName", "test-cloud" },
                { "Cloudinary:ApiKey", "123456" },
                { "Cloudinary:ApiSecret", "abcdef" }
            };

            config.AddInMemoryCollection(testConfig);
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<EcoBridgeDbContext>));
            services.AddDbContext<EcoBridgeDbContext>(options =>
                options.UseSqlServer(_containersFixture.GetSqlConnectionString()));

            services.RemoveAll(typeof(IConnectionMultiplexer));
            services.AddSingleton<IConnectionMultiplexer>(_ => 
                ConnectionMultiplexer.Connect(_containersFixture.GetRedisConnectionString()));

            services.RemoveAll(typeof(IEmailService));
            services.AddSingleton(EmailServiceMock.Object);

            services.RemoveAll(typeof(ICloudinaryService));
            services.AddSingleton(CloudinaryServiceMock.Object);

            // Configure JWT for test environment
            services.Configure<JWTSettings>(options =>
            {
                options.SecretKey = "TestSuperSecretKeyForE2ETesting20261234567890!";
                options.Issuer = "EcoBridgeAPI";
                options.Audience = "EcoBridgeClients";
                options.DurationInMinutes = 60;
                options.RefreshTokenDurationInDays = 30;
            });

            services.PostConfigure<Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerOptions>(
                Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme,
                options =>
                {
                    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = "EcoBridgeAPI",
                        ValidAudience = "EcoBridgeClients",
                        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                            System.Text.Encoding.UTF8.GetBytes("TestSuperSecretKeyForE2ETesting20261234567890!"))
                    };
                });
        });
    }
}
