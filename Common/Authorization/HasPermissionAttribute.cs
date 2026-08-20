using Microsoft.AspNetCore.Authorization;

namespace DulceAtardecer.Common.Authorization;

public class HasPermissionAttribute(string permission) : AuthorizeAttribute(policy: permission);
