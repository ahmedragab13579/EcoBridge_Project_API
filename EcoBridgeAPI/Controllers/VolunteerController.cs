using EcoBridge.Domain.Errors;
using EcoBridge.Application.DTO;
using EcoBridge.Application.Features.Donation.Queries;
using EcoBridge.Application.Features.Volunteer.Commands;
using EcoBridge.Application.Features.Volunteer.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcoBridge.API.Controllers;

[Route("api/volunteers")]
[ApiController]
public class VolunteerController(ISender sender) : BaseApiController
{
    private readonly ISender _sender = sender;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _sender.Send(new GetVolunteersQuery());
        return HandleResult(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _sender.Send(new GetVolunteerByIdQuery(id));
        return HandleResult(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(CreateVolunteerDTO dto)
    {
        var result = await _sender.Send(new CreateVolunteerCommand(dto.AccountId, dto));
        return HandleResult(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, CreateVolunteerDTO dto)
    {
        var result = await _sender.Send(new UpdateVolunteerCommand(id, dto));
        return HandleResult(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _sender.Send(new DeleteVolunteerCommand(id));
        return HandleResult(result);
    }

    [HttpGet("available-tasks")]
    [Authorize(Roles = "Volunteer,Admin")]
    public async Task<IActionResult> GetAvailableTasks()
    {
        var result = await _sender.Send(new GetAvailableTasksQuery(), HttpContext.RequestAborted);
        return HandleResult(result);
    }

    [HttpGet("my-tasks")]
    [Authorize(Roles = "Volunteer")]
    public async Task<IActionResult> GetMyTasks()
    {
        var volunteerId = CurrentUserId;
        if (volunteerId == Guid.Empty)
            return ApiUnauthorized(AppMessageCodes.UNAUTHORIZED);

        var result = await _sender.Send(new GetVolunteerTasksQuery(volunteerId), HttpContext.RequestAborted);
        return HandleResult(result);
    }

    [HttpPatch("me/availability")]
    [Authorize(Roles = "Volunteer")]
    public async Task<IActionResult> ToggleMyAvailability([FromQuery] bool isAvailable)
    {
        var volunteerId = CurrentUserId;
        if (volunteerId == Guid.Empty)
            return ApiUnauthorized(AppMessageCodes.UNAUTHORIZED);

        var result = await _sender.Send(new ToggleVolunteerAvailabilityCommand(volunteerId, isAvailable));
        return HandleResult(result);
    }
}
