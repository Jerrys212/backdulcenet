using Microsoft.AspNetCore.Identity;

namespace DulceAtardecer.Models;

public class ApplicationUser : IdentityUser
{
    public string Nombre { get; set; } = string.Empty;
}
