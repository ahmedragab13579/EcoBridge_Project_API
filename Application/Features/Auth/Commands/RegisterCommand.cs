using EcoBridge.Application.DTO.Users.Auth.Users;
using EcoBridge.Application.Result;
using MediatR;

namespace EcoBridge.Application.Features.Auth.Commands;

public record RegisterCommand(RegisterRequestDTO Request) : IRequest<ApiResult<AuthResponseDTO>>;
