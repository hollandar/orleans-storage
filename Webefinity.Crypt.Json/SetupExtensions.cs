using Microsoft.Extensions.DependencyInjection;

namespace Webefinity.Crypt.Json;

public static class SetupExtensions
{
    public static IServiceCollection AddOnDiskEncryptedKeyValueService(this IServiceCollection services)
    {
        services.AddSingleton<IEncryptedKeyValueService, EncryptedOnDiskKeyValueService>();
        return services;
    }
}
