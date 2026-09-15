using EcoBridge.Application.Contracts;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Models;
using EcoBridge.Domain.Results;
using EcoBridge.Application.DTO;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Microsoft.AspNetCore.Identity;

// ... existing usings

namespace EcoBridge.Application.Features.Volunteer.Commands;

public record CreateVolunteerCommand(
    Guid AccountId,
    CreateVolunteerDTO Dto
) : IRequest<Result<Guid>>;

public class CreateVolunteerCommandHandler(
    IEcoBridgeDbContext dbContext,
    IPasswordHasher<EcoBridge.Domain.Models.Account> passwordHasher,
    IEmailService emailService
) : IRequestHandler<CreateVolunteerCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateVolunteerCommand request, CancellationToken cancellationToken)
    {
        var exists = await dbContext.Volunteers.AnyAsync(v => v.AccountId == request.AccountId, cancellationToken);
        if (exists)
            return Result<Guid>.FailResult(Guid.Empty, AppMessageCodes.CONFLICT, ErrorType.Conflict);

        var accountId = request.AccountId;

        var accountExists = await dbContext.Accounts.AnyAsync(a => a.Id == accountId, cancellationToken);

        // If account does not exist and admin provided account details, create the account
        if (!accountExists)
        {
            // Require basic account info
            if (string.IsNullOrWhiteSpace(request.Dto?.Email) || string.IsNullOrWhiteSpace(request.Dto?.FullName) || string.IsNullOrWhiteSpace(request.Dto?.Phone))
            {
                return Result<Guid>.FailResult(Guid.Empty, AppMessageCodes.ACCOUNT_NOT_FOUND, ErrorType.NotFound);
            }

            var password = string.IsNullOrWhiteSpace(request.Dto.Password) ? GenerateTemporaryPassword() : request.Dto.Password!;
            var passwordHash = passwordHasher.HashPassword(null!, password);

            var account = EcoBridge.Domain.Models.Account.Create(request.Dto.FullName!, request.Dto.Email!, request.Dto.Phone!, passwordHash, EcoBridge.Domain.Enums.UserRole.Volunteer);
            dbContext.Accounts.Add(account);
            await dbContext.SaveChangesAsync(cancellationToken);

            accountId = account.Id;

            // Send temporary credentials to the new volunteer
            try
            {
                await emailService.SendNewAccountEmailAsync(request.Dto.Email!, password, cancellationToken);
            }
            catch (Exception ex)
            {
                // Log & continue - account creation succeeded; avoid failing the whole request due to email issues
                // Using Console.WriteLine for minimal coupling; logging infrastructure could be injected if needed.
                Console.WriteLine($"Failed sending new account email to {request.Dto.Email}: {ex.Message}");
            }
        }

        var volunteer = EcoBridge.Domain.Models.Volunteer.Create(accountId, null, request.Dto.VehicleDetails);

        dbContext.Volunteers.Add(volunteer);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<Guid>.SuccessResult(volunteer.AccountId, AppMessageCodes.VOLUNTEER_CREATED);
    }

    private static string GenerateTemporaryPassword()
    {
        // simple random alphanumeric password generator (8 chars)
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789";
        var rnd = new Random();
        return new string(Enumerable.Range(0, 10).Select(_ => chars[rnd.Next(chars.Length)]).ToArray());
    }
}
