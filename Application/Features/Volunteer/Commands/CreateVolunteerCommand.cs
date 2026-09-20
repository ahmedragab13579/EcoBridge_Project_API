using EcoBridge.Application.Contracts;
using EcoBridge.Application.DTO;
using EcoBridge.Application.Result;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Models.Accounts;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

// ... existing usings

namespace EcoBridge.Application.Features.Volunteer.Commands;

public record CreateVolunteerCommand(
    Guid AccountId,
    CreateVolunteerDTO Dto
) : IRequest<ApiResult<Guid>>;

public class CreateVolunteerCommandHandler(
    IEcoBridgeDbContext dbContext,
    IPasswordHasher<Account> passwordHasher,
    IEmailService emailService
) : IRequestHandler<CreateVolunteerCommand, ApiResult<Guid>>
{
    public async Task<ApiResult<Guid>> Handle(CreateVolunteerCommand request, CancellationToken cancellationToken)
    {
        var accountId = request.AccountId;

        var account = await dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken);

        if (account == null)
        {
            // Require basic account info
            if (string.IsNullOrWhiteSpace(request.Dto?.Email) || string.IsNullOrWhiteSpace(request.Dto?.FullName) || string.IsNullOrWhiteSpace(request.Dto?.Phone))
            {
                return ApiResult<Guid>.Fail(AppMessageCodes.ACCOUNT_NOT_FOUND);
            }

            var password = string.IsNullOrWhiteSpace(request.Dto.Password) ? GenerateTemporaryPassword() : request.Dto.Password!;
            var passwordHash = passwordHasher.HashPassword(null!, password);

            account = Account.Create(
                request.Dto.FullName!,
                request.Dto.Email!,
                request.Dto.Phone!,
                passwordHash,
                EcoBridge.Domain.Enums.UserRole.Volunteer,
                vehicleType: null,
                vehicleDetails: request.Dto.VehicleDetails);

            dbContext.Accounts.Add(account);
            await dbContext.SaveChangesAsync(cancellationToken);

            try
            {
                await emailService.SendNewAccountEmailAsync(request.Dto.Email!, password, cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed sending new account email to {request.Dto.Email}: {ex.Message}");
            }
        }
        else
        {
            account.SetVehicleInfo(request.Dto.VehicleDetails, request.Dto.VehicleDetails);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return ApiResult<Guid>.Ok(account.Id, AppMessageCodes.VOLUNTEER_CREATED);
    }

    private static string GenerateTemporaryPassword()
    {
        // simple random alphanumeric password generator (8 chars)
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789";
        var rnd = new Random();
        return new string(Enumerable.Range(0, 10).Select(_ => chars[rnd.Next(chars.Length)]).ToArray());
    }
}
