using EcoBridge.Application.DTO.Users.Auth.Users;
using EcoBridge.Application.Result;
using EcoBridge.Domain.Errors;
using MediatR;

namespace EcoBridge.Application.Features.Auth.Commands;

public record RefreshTokenCommand(RefreshTokenRequestDTO Request) : IRequest<ApiResult<AuthResponseDTO>>;
