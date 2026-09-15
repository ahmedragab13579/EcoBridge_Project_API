using EcoBridge.Domain.Models;
using Microsoft.AspNetCore.Identity;

namespace EcoBridge.E2ETests.Common;

public static class TestPasswordHelper
{
    private static readonly PasswordHasher<Account> _hasher = new();

    public static string Hash(string password = "TestPassword123!") => _hasher.HashPassword(null!, password);
    public static PasswordHasher<Account> Hasher => _hasher;
}
