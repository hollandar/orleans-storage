using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Webefinity.Module.Messaging.Data;

public static class SetupExtensions
{
    public static void AddMessagingDbContext<TDbContext>(this IServiceCollection services) where TDbContext : DbContext
    {
        services.AddScoped<IMessagingDbContext, MessagingDbContext<TDbContext>>(sp =>
        {
            var dbContext = sp.GetRequiredService<TDbContext>();
            return new MessagingDbContext<TDbContext>(dbContext);
        });
    }
}
