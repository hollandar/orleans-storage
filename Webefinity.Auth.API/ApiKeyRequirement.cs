using Microsoft.AspNetCore.Authorization;

namespace Webefinity.Auth.API;

public class ApiKeyRequirement : IAuthorizationRequirement
{
    public string HeaderName { get; }
    public string Key { get; }

    public ApiKeyRequirement(string headerName, string key)
    {
        HeaderName = headerName;
        Key = key;
    }
}
