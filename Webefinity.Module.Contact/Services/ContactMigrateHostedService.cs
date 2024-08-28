using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Webefinity.Module.Contact.Data;

namespace Webefinity.Module.Contact.Services
{
    internal class ContactMigrateHostedService : IHostedService
    {
        private readonly IServiceProvider serviceProvider;

        public ContactMigrateHostedService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // Resolve a db context
            using var scope = this.serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ContactDbContext>();

            // Set up a database
            await dbContext.Database.MigrateAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
