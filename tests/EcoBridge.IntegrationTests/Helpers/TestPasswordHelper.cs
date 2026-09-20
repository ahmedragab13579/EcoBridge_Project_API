using EcoBridge.Domain.Models.Accounts;
using Microsoft.AspNetCore.Identity;

namespace EcoBridge.IntegrationTests.Helpers;

public static class TestPasswordHelper
{
    private static readonly PasswordHasher<Account> _hasher = new();

    public static string Hash(string password = "TestPassword123!") => _hasher.HashPassword(null!, password);
    public static PasswordHasher<Account> Hasher => _hasher;
}
