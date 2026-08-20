using Microsoft.AspNetCore.Authorization;

namespace DulceAtardecer.Common.Authorization;

public class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}
