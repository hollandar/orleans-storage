using System.Security.Claims;

namespace Webefinity.Authorization.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static string Subject(this ClaimsPrincipal principal)
    {
        return 
            principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? 
            principal.FindFirstValue("sub") ??
            throw new Exception("NameIdentifier/sub not found.");
    }

    public static string Name(this ClaimsPrincipal principal)
    {
        return 
            principal.FindFirstValue(ClaimTypes.Name) ?? 
            principal.FindFirstValue("name") ??
            throw new Exception("Name/name not found.");
    }
}
