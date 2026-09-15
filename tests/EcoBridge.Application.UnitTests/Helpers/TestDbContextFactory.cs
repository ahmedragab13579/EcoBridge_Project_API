using System;
using EcoBridge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.UnitTests.Helpers;

public static class TestDbContextFactory
{
    public static EcoBridgeDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<EcoBridgeDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new EcoBridgeDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }
}
