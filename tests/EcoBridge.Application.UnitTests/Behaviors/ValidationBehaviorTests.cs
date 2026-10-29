using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EcoBridge.Application.DTO;
using EcoBridge.Application.DTO.Users;
using EcoBridge.Application.DTO.Users.Auth;
using EcoBridge.Application.DTO.Users.Auth.Users;
using EcoBridge.Application.Extensions;
using EcoBridge.Application.Features.Auth.Commands;
using EcoBridge.Application.Result;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;
using FluentAssertions;
using FluentValidation;
using MediatR;
using Xunit;

namespace EcoBridge.Application.UnitTests.Behaviors;

public class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_Should_Invoke_Next_When_No_Validators_Are_Registered()
    {
        var behavior = new ValidationBehavior<LoginCommand, ApiResult<AuthResponseDTO>>(
            new List<IValidator<LoginCommand>>());
        var nextCalled = false;

        var result = await behavior.Handle(
            new LoginCommand(new LoginRequestDTO()),
            () =>
            {
                nextCalled = true;
                return Task.FromResult(ApiResult<AuthResponseDTO>.Ok(new AuthResponseDTO()));
            },
            CancellationToken.None);

        nextCalled.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Should_Throw_ValidationException_When_Request_Is_Invalid()
    {
        var validator = new InlineValidator<LoginCommand>();
        validator.RuleFor(x => x.Request).NotNull();
        var behavior = new ValidationBehavior<LoginCommand, ApiResult<AuthResponseDTO>>(
            new[] { validator });
        var nextCalled = false;

        var action = () => behavior.Handle(
            new LoginCommand(null!),
            () =>
            {
                nextCalled = true;
                return Task.FromResult(ApiResult<AuthResponseDTO>.Ok(new AuthResponseDTO()));
            },
            CancellationToken.None);

        var exception = await action.Should().ThrowAsync<DomainException.ValidationException>();
        exception.Which.Code.Should().Be(AppMessageCodes.VALIDATION_ERROR);
        nextCalled.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_Should_Invoke_Next_When_Request_Is_Valid()
    {
        var validator = new InlineValidator<LoginCommand>();
        validator.RuleFor(x => x.Request).NotNull();
        var behavior = new ValidationBehavior<LoginCommand, ApiResult<AuthResponseDTO>>(
            new[] { validator });
        var request = new LoginCommand(new LoginRequestDTO());

        var result = await behavior.Handle(
            request,
            () => Task.FromResult(ApiResult<AuthResponseDTO>.Ok(new AuthResponseDTO())),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }
}
