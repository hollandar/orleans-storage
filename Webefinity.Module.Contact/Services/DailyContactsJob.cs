using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Webefinity.Module.Contact.Data;
using Webefinity.Module.Contact.Interfaces;
using Webefinity.Module.Contact.Models;

namespace Webefinity.Module.Contact.Services
{
    public class DailyContactsJob : IJob
    {
        static SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        private readonly ContactDbContext dbContext;
        private readonly IServiceProvider serviceProvider;

        public DailyContactsJob(ContactDbContext dbContext, IServiceProvider serviceProvider, IEnumerable<IContactMessageFormatter> formatters)
        {
            this.dbContext = dbContext;
            this.serviceProvider = serviceProvider;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            using var scope = this.serviceProvider.CreateScope();
            var logger = scope.ServiceProvider.GetService<ILogger<DailyContactsJob>>();
            await semaphore.WaitAsync();
            try
            {
                if (dbContext.Contacts.Count() == 0)
                {
                    logger.LogInformation("No contacts to send.");
                    return;
                }

                var contacts = dbContext.Contacts.ToList();
                dbContext.Contacts.RemoveRange(contacts);

                var contactGroups = contacts.GroupBy(r => r.Type);
                var formatters = scope.ServiceProvider.GetServices<IContactMessageFormatter>();
                var sender = scope.ServiceProvider.GetRequiredService<ISendContactService>();
                foreach (var group in contactGroups)
                {
                    var formatter = formatters.Where(f => f.HandlesType(group.Key)).FirstOrDefault();
                    if (formatter is null)
                    {
                        logger.LogError("Could not handle contact type {ContactType}", group.Key);
                        continue;
                    }

                    var contactRecords = group.Select(r => new ContactModel { Body = r.Data, Id = r.Id, Type = r.Type });
                    var formattedMessage = formatter.FormatMessage(contactRecords.ToList());

                    await sender.SendAsync(formattedMessage);
                }

            }
            finally
            {
                await dbContext.SaveChangesAsync();
                semaphore.Release();
            }
        }
    }
}
