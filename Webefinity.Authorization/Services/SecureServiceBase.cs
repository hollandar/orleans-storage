using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Security.Principal;
using Webefinity.Authorization.Extensions;

namespace Webefinity.Authorization.Services;

public class SecureServiceBase
{
    private readonly IAuthorizationService authorizationService;
    private readonly IHttpContextAccessor httpContextAccessor;

    public SecureServiceBase(IAuthorizationService authorizationService, IHttpContextAccessor httpContextAccessor)
    {
        this.authorizationService = authorizationService;
        this.httpContextAccessor = httpContextAccessor;
    }

    protected IAuthorizationService AuthorizationService => authorizationService;
    protected IHttpContextAccessor HttpContextAccessor => httpContextAccessor;
    protected ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User ?? null;

    protected Task<bool> IsAuthenticatedAsync()
    {
        return Task.FromResult(httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false);
    }

    protected async Task<(bool Success, AuthorizationResult? Result)> IsAuthorizedAsync(string policyName)
    {
        if (!(httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false))
        {
            return (false, null);
        }

        var result = await authorizationService.AuthorizeAsync(httpContextAccessor.HttpContext.User, policyName);
        return (result.Succeeded, result);
    }
    
    public string Subject()
    {
        ArgumentNullException.ThrowIfNull(this.HttpContextAccessor.HttpContext);
        if (this.httpContextAccessor.HttpContext!.User.Identity?.IsAuthenticated ?? false) { 
            return this.HttpContextAccessor.HttpContext.User.Subject();
        }

        return string.Empty;
    }
}
