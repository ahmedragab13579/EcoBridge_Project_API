using EcoBridge.Application.DTO;
using EcoBridge.Application.Result;
using EcoBridge.Domain.Errors;
using MediatR;

namespace EcoBridge.Application.Features.Auth.Commands;

public record LoginCommand(LoginRequestDTO Request) : IRequest<ApiResult<AuthResponseDTO>>;
