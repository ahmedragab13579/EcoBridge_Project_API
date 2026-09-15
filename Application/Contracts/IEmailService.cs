namespace EcoBridge.Application.Contracts;

public interface IEmailService
{
    Task SendPasswordResetEmailAsync(string email, string token, CancellationToken ct = default);
    Task SendNewAccountEmailAsync(string email, string temporaryPassword, CancellationToken ct = default);
}
