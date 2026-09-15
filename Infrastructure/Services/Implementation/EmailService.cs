using EcoBridge.Application.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using sib_api_v3_sdk.Api;
using sib_api_v3_sdk.Client;
using sib_api_v3_sdk.Model;
using Task = System.Threading.Tasks.Task;

namespace EcoBridge.Infrastructure.Services.Implementation;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly IConfiguration _configuration;

    public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task SendNewAccountEmailAsync(string email, string temporaryPassword, CancellationToken ct = default)
    {
        _logger.LogInformation("Sending new account email to {Email}", email);

        var apiKey = _configuration["BrevoSettings:ApiKey"];
        var fromEmail = _configuration["BrevoSettings:FromEmail"];
        var fromName = _configuration["BrevoSettings:FromName"] ?? "EcoBridge App";

        if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(fromEmail))
        {
            _logger.LogWarning("Brevo Email settings are incomplete. Skipping new account email sending.");
            return;
        }

        Configuration.Default.ApiKey["api-key"] = apiKey;
        var apiInstance = new TransactionalEmailsApi();

        var sender = new SendSmtpEmailSender(fromName, fromEmail);
        var to = new List<SendSmtpEmailTo> { new SendSmtpEmailTo(email) };

        var frontendUrl = _configuration["Frontend:LoginUrl"] 
                          ?? _configuration["FrontendUrl"] 
                          ?? "http://localhost:5173/login";

        var sendSmtpEmail = new SendSmtpEmail(
            sender: sender,
            to: to,
            subject: "Welcome to EcoBridge - Your account is ready",
            htmlContent: $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #2e7d32;'>Welcome to EcoBridge</h2>
                    <p>Your volunteer account has been created by an administrator. Use the credentials below to sign in and change your password.</p>
                    <p><strong>Email:</strong> {email}</p>
                    <p><strong>Temporary Password:</strong> {temporaryPassword}</p>
                    <p style='margin-top: 20px;'>Sign in here: <a href='{frontendUrl}'>{frontendUrl}</a></p>
                    <p style='margin-top: 30px; font-size: 12px; color: #888;'>For your safety, please change your password after signing in.</p>
                </div>",
            textContent: $"Your EcoBridge account has been created. Email: {email} Password: {temporaryPassword}"
        );

        try
        {
            await apiInstance.SendTransacEmailAsync(sendSmtpEmail);
            _logger.LogInformation("New account email sent successfully to {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send new account email to {Email}", email);
            throw;
        }
    }

    public async Task SendPasswordResetEmailAsync(string email, string token, CancellationToken ct = default)
    {
        _logger.LogInformation("Sending password reset email to {Email}. Token: {Token}", email, token);

        var apiKey = _configuration["BrevoSettings:ApiKey"];
        var fromEmail = _configuration["BrevoSettings:FromEmail"];
        var fromName = _configuration["BrevoSettings:FromName"] ?? "EcoBridge App";

        if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(fromEmail))
        {
            _logger.LogWarning("Brevo Email settings are incomplete. Skipping email sending.");
            return;
        }

        Configuration.Default.ApiKey["api-key"] = apiKey;
        var apiInstance = new TransactionalEmailsApi();

        var sender = new SendSmtpEmailSender(fromName, fromEmail);
        var to = new List<SendSmtpEmailTo> { new SendSmtpEmailTo(email) };

        var frontendUrl = _configuration["Frontend:ResetPasswordUrl"] 
                          ?? _configuration["FrontendUrl"] 
                          ?? "http://localhost:5173/reset-password";
        
        var encodedToken = Uri.EscapeDataString(token);
        var encodedEmail = Uri.EscapeDataString(email);
        var resetLink = $"{frontendUrl}?email={encodedEmail}&token={encodedToken}";

        var sendSmtpEmail = new SendSmtpEmail(
            sender: sender,
            to: to,
            subject: "EcoBridge - Password Reset Request",
            htmlContent: $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #2e7d32;'>EcoBridge Password Reset</h2>
                    <p>You recently requested to reset your password for your EcoBridge account.</p>
                    <p>Click the button below to proceed to the password reset page:</p>
                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='{resetLink}' style='background-color: #2e7d32; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px; font-weight: bold;'>Reset Password</a>
                    </div>
                    <p>Or copy and paste this URL into your browser:</p>
                    <p style='color: #555; word-break: break-all;'>{resetLink}</p>
                    <p style='margin-top: 30px; font-size: 12px; color: #888;'>If you did not request a password reset, please ignore this email.</p>
                </div>",
            textContent: $"Please reset your password by visiting the following link: {resetLink}"
        );

        try
        {
            await apiInstance.SendTransacEmailAsync(sendSmtpEmail);
            _logger.LogInformation("Password reset email sent successfully to {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password reset email to {Email}", email);
            throw;
        }
    }
}