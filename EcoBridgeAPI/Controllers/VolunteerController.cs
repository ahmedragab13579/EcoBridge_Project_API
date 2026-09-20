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
        return ApiOk(result.Data!, result.Code);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _sender.Send(new GetVolunteerByIdQuery(id));
        return ApiOk(result.Data!, result.Code);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(CreateVolunteerDTO dto)
    {
        var result = await _sender.Send(new CreateVolunteerCommand(dto.AccountId, dto));
        return ApiCreated(
            result.Data!,
            actionName: nameof(GetById),
            routeValues: new { id =1 },
            code: result.Code);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, CreateVolunteerDTO dto)
    {
        var result = await _sender.Send(new UpdateVolunteerCommand(id, dto));
        return ApiOk(result.Data!, result.Code);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _sender.Send(new DeleteVolunteerCommand(id));
        return ApiOkWithCode(result.Code);
    }

    [HttpGet("available-tasks")]
    [Authorize(Roles = "Volunteer,Admin")]
    public async Task<IActionResult> GetAvailableTasks()
    {
        var result = await _sender.Send(
            new GetAvailableTasksQuery(),
            HttpContext.RequestAborted);

        return ApiOk(result.Data!, result.Code);
    }

    [HttpGet("my-tasks")]
    [Authorize(Roles = "Volunteer")]
    public async Task<IActionResult> GetMyTasks()
    {
        var result = await _sender.Send(
            new GetVolunteerTasksQuery(CurrentUserId),
            HttpContext.RequestAborted);

        return ApiOk(result.Data!, result.Code);
    }

    [HttpPatch("me/availability")]
    [Authorize(Roles = "Volunteer")]
    public async Task<IActionResult> ToggleMyAvailability([FromQuery] bool isAvailable)
    {
        var result = await _sender.Send(
            new ToggleVolunteerAvailabilityCommand(CurrentUserId, isAvailable));

        return ApiOk(result.Data!, result.Code);
    }
}