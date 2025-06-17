using Microsoft.Extensions.Options;
using Webefinity.Auth.API.Options;

namespace Webefinity.Auth.API;

public class OptionsConfigurationProvider : IAPIKeyProvider
{
    public IOptions<ApiKeyOptions> options { get; }

    public OptionsConfigurationProvider(IOptions<ApiKeyOptions> options)
    {
        this.options = options;
    }

    public string GetEndpoint()
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(options.Value.Endpoint, nameof(options.Value.Endpoint));
        return options.Value.Endpoint;
    }

    public string[] GetKeyStrings()
    {
        return options.Value.ApiKeys;
    }
}

