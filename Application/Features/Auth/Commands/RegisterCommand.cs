using EcoBridge.Application.DTO;
using EcoBridge.Application.Result;
using MediatR;

namespace EcoBridge.Application.Features.Auth.Commands;

public record RegisterCommand(RegisterRequestDTO Request) : IRequest<ApiResult<AuthResponseDTO>>;
