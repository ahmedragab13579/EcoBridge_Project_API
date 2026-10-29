using EcoBridge.Application.DTO.Users.Charity;
using EcoBridge.Application.Result;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace EcoBridge.Application.Features.Auth.Commands.Profiles
{
    public record UpdateCharityProfileCommand(Guid UserId,UpdateCharityProfileDTO updateCharityProfileDTO) : IRequest<ApiResult<CharityProfileDTO>>
    {
    }
}
