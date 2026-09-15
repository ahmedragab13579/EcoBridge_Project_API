using EcoBridge.Application.Contracts;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Models;
using EcoBridge.Domain.Results;
using Hangfire;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Donation.Commands;

public class RequestDonationCommandHandler(
    IBackgroundJobClient _backgroundJobClient,
IEcoBridgeDbContext dbContext
) : IRequestHandler<RequestDonationCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(RequestDonationCommand request, CancellationToken cancellationToken)
    {
        var charity = await dbContext.Charities
            .FirstOrDefaultAsync(c => c.AccountId == request.CharityAccountId, cancellationToken);
        if (charity == null)
            return Result<Guid>.FailResult(Guid.Empty, AppMessageCodes.CHARITY_NOT_FOUND, ErrorType.NotFound);

        var donation = await dbContext.Donations
            .FirstOrDefaultAsync(d => d.Id == request.DonationId, cancellationToken);
        if (donation == null)
            return Result<Guid>.FailResult(Guid.Empty, AppMessageCodes.DONATION_NOT_FOUND, ErrorType.NotFound);

        if (donation.CharityId != null)
            return Result<Guid>.FailResult(Guid.Empty, AppMessageCodes.DONATION_ALREADY_REQUESTED, ErrorType.Conflict);

        if (donation.Status != DonationStatus.Available && donation.Status != DonationStatus.Pending)
            return Result<Guid>.FailResult(Guid.Empty, AppMessageCodes.DONATION_CANNOT_BE_MODIFIED, ErrorType.Conflict);

        // Mark donation requested for this charity
        donation.MarkAsAccepted(charity.AccountId);

        var donationRequest = DonationRequest.Create(donation.Id, charity.AccountId);

        dbContext.DonationRequests.Add(donationRequest);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<Guid>.FailResult(Guid.Empty, AppMessageCodes.CONCURRENCY_CONFLICT, ErrorType.Conflict);
        }

        //Schedule Hangfire 12 - Hour Charity Approval Timeout SLA Job
        _backgroundJobClient.Schedule<BackgroundJobs.CharityApprovalTimeoutJob>(
            job => job.ExecuteAsync(donationRequest.Id),
            TimeSpan.FromHours(12)
        );

        return Result<Guid>.SuccessResult(donationRequest.Id, AppMessageCodes.DONATION_REQUEST_CREATED);
    }
}
