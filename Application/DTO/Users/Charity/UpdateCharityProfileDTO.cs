using EcoBridge.Application.DTO.Users.Auth.Users.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace EcoBridge.Application.DTO.Users.Charity
{
    public class UpdateCharityProfileDTO : MiniUpdateDto
    {

        public string? OrganizationName { get;  set; }

        public string? RegistrationNumber { get;  set; }
    }
}
