using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Webefinity.Auth.API.Options;

namespace Webefinity.Auth.API;

public class ApiKeyHandler : AuthorizationHandler<ApiKeyRequirement>
{
    private readonly IServiceProvider sp;
    private readonly IHttpContextAccessor httpContextAccessor;

    public ApiKeyHandler(IServiceProvider sp, IHttpContextAccessor httpContextAccessor)
    {
        this.sp = sp;
        this.httpContextAccessor = httpContextAccessor;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ApiKeyRequirement requirement)
    {
        var keyProvider = sp.GetRequiredKeyedService<IAPIKeyProvider>(requirement.Key);
        var httpContext = httpContextAccessor.HttpContext;
        ArgumentNullException.ThrowIfNull(httpContext, nameof(httpContext));

        if (httpContext.Request.Headers.TryGetValue(requirement.HeaderName, out var apiKey) && !string.IsNullOrEmpty(apiKey))
        {
            if (!String.IsNullOrWhiteSpace(apiKey) && keyProvider.GetKeyStrings().Any(r => r.Equals(apiKey)))
            {
                context.Succeed(requirement);
            }
        }

        return Task.CompletedTask;
    }
}
