using EcoBridge.Application.DTO.Users.Auth;
using EcoBridge.Application.DTO.Users.Auth.Users.Common;
using EcoBridge.Application.DTO.Users.Volunteer;
using EcoBridge.Application.Result;
using MediatR;

namespace EcoBridge.Application.Features.Auth.Commands.Profiles;

public record UpdateVolunteerProfileCommand(Guid UserId, UpdateVolunteerProfileDTO Request) : IRequest<ApiResult<VolunteerProfileDTO>>;
