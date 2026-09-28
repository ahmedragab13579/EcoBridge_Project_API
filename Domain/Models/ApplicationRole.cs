using Microsoft.AspNetCore.Identity;

namespace EcoBridge.Domain.Models;

public class ApplicationRole:IdentityRole<Guid>
{
    private ApplicationRole()
    {
        
    }


    public ApplicationRole(string name) : base(name)
    {

    }
}
