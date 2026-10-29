using EcoBridge.Application.DTO.Users.Auth.Users;
using EcoBridge.Application.Result;
using EcoBridge.Domain.Errors;
using MediatR;

namespace EcoBridge.Application.Features.Auth.Commands;

public record ChangeUserPasswordCommand(Guid UserId, ChangePasswordDTO Request) : IRequest<ApiResult<bool>>;
