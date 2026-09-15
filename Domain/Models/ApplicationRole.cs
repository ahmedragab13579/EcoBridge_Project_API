using Microsoft.AspNetCore.Identity;

namespace EcoBridge.Domain.Models;

public class ApplicationRole : IdentityRole<Guid>
{
    public ApplicationRole() { }
    public ApplicationRole(string roleName) : base(roleName) { }
}
