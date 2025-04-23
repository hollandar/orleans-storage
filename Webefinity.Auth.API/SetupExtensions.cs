using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Webefinity.Auth.API.Options;

namespace Webefinity.Auth.API
{
    public static class SetupExtensions
    {
        private const string DefaultHeaderName = "X-API-KEY";
        private const string DefaultSectionName = "ApiKeyOptions";
        private const string DefaultKey = "ApiKey";

        public static void AddApiKeyOptionsProvider(this IServiceCollection services, string key = DefaultKey,  string sectionName = DefaultSectionName)
        {
            services.AddKeyedSingleton<IAPIKeyProvider>(key, (sp, _) =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                var configSection = configuration.GetSection(sectionName);
                var options = new ApiKeyOptions();
                configSection.Bind(options);
                var optionsInstance = Microsoft.Extensions.Options.Options.Create(options);
                return new OptionsConfigurationProvider(optionsInstance);
            });
        }

        public static void AddApiKeyAuthorization(this IServiceCollection services, string policy, string key = DefaultKey, string header = DefaultHeaderName)
        {
            services.AddSingleton<IAuthorizationHandler, ApiKeyHandler>();
            services.AddAuthorization(options =>
            {
                options.AddPolicy(policy, policyOptions => policyOptions.AddRequirements(new ApiKeyRequirement(header, key)));
            });
        }

        public static void AddApiKeyClientProvider(this IServiceCollection services, string key = DefaultKey, string header = DefaultHeaderName)
        {
            services.AddKeyedScoped<IApiKeyClientProvider>(key, (sp, _) => {
                var apiKeyProvider = sp.GetRequiredKeyedService<IAPIKeyProvider>(key);
                var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                return new HttpClientProviderService(httpClientFactory, apiKeyProvider, header);
            } );
        }

    }
}
