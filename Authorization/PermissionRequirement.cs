using Microsoft.AspNetCore.Authorization;

namespace DulceAtardecer.Authorization;

public class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}
