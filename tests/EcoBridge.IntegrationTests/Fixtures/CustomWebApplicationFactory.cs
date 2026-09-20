using EcoBridge.Application.Contracts;
using EcoBridge.Application.Services;
using EcoBridge.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using StackExchange.Redis;
using Xunit;

namespace EcoBridge.IntegrationTests.Fixtures;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly EcoBridgeContainersFixture _containersFixture;

    public Mock<IEmailService> EmailServiceMock { get; } = new();
    public Mock<ICloudinaryService> CloudinaryServiceMock { get; } = new();

    public CustomWebApplicationFactory()
    {
        _containersFixture = new EcoBridgeContainersFixture();
    }

    public async Task InitializeAsync()
    {
        await _containersFixture.InitializeAsync();

        // Run migrations
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EcoBridgeDbContext>();
        await db.Database.MigrateAsync();
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
                { "Redis:InstancePrefix", "EcoBridgeTest:" },
                { "JWT:SecretKey", "TestSuperSecretKeyForIntegrationTesting20261234567890!" },
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
            // Replace DbContext
            services.RemoveAll(typeof(DbContextOptions<EcoBridgeDbContext>));
            services.AddDbContext<EcoBridgeDbContext>(options =>
                options.UseSqlServer(_containersFixture.GetSqlConnectionString()));

            // Replace Redis Multiplexer
            services.RemoveAll(typeof(IConnectionMultiplexer));
            services.AddSingleton<IConnectionMultiplexer>(_ => 
                ConnectionMultiplexer.Connect(_containersFixture.GetRedisConnectionString()));

            // Replace mockable external services
            services.RemoveAll(typeof(IEmailService));
            services.AddSingleton(EmailServiceMock.Object);

            services.RemoveAll(typeof(ICloudinaryService));
            services.AddSingleton(CloudinaryServiceMock.Object);

            // Configure JWT for test environment
            services.Configure<JWTSettings>(options =>
            {
                options.SecretKey = "TestSuperSecretKeyForIntegrationTesting20261234567890!";
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
                            System.Text.Encoding.UTF8.GetBytes("TestSuperSecretKeyForIntegrationTesting20261234567890!"))
                    };
                });
        });
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EcoBridgeDbContext>();
        // Clean relevant tables if needed between test executions
        await db.Database.EnsureCreatedAsync();
    }
}
