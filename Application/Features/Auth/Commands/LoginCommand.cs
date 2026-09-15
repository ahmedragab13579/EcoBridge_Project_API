using EcoBridge.Application.DTO;
using EcoBridge.Domain.Results;
using EcoBridge.Domain.Errors;
using MediatR;

namespace EcoBridge.Application.Features.Auth.Commands;

public record LoginCommand(LoginRequestDTO Request) : IRequest<Result<AuthResponseDTO>>;
