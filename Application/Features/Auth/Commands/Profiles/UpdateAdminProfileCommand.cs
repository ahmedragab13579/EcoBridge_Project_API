using EcoBridge.Application.DTO.Users.Auth.Users.Common;
using EcoBridge.Application.Result;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace EcoBridge.Application.Features.Auth.Commands.Profiles
{
    public record UpdateDonorAdminProfileCommand(Guid id,MiniUpdateDto request) : IRequest<ApiResult<UserDTO>>
    {
    }
}
