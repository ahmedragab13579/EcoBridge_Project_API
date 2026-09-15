using EcoBridge.Application.DTO;
using EcoBridge.Domain.Results;
using EcoBridge.Domain.Errors;
using MediatR;

namespace EcoBridge.Application.Features.Auth.Commands;

public record ChangeUserPasswordCommand(Guid UserId, ChangePasswordDTO Request) : IRequest<Result<bool>>;
