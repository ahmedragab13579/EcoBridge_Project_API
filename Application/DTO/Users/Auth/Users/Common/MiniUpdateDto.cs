using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EcoBridge.Application.DTO.Users.Auth.Users.Common
{
    public class MiniUpdateDto
    {

        public string FullName { get; set; } = string.Empty;


        public string Phone { get; set; } = string.Empty;

        public int? CityId { get; set; }

        public string? Address { get; set; }

        public string? AddressDetails { get; set; }

        public IFormFile? ProfileImage { get; set; }

    }
}
