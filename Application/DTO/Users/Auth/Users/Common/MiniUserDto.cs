using System;
using System.Collections.Generic;
using System.Text;

namespace EcoBridge.Application.DTO.Users.Auth.Users.Common
{
    public class MiniUserDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string UserType { get; set; } = string.Empty;
        public string? ImagePath { get; set; }

    }
}
