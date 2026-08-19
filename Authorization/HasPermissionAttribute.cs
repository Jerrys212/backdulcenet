using Microsoft.AspNetCore.Authorization;

namespace DulceAtardecer.Authorization;

public class HasPermissionAttribute(string permission) : AuthorizeAttribute(policy: permission);
