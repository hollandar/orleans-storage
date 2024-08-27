using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Webefinity.Module.Contact.Data;
using Webefinity.Module.Contact.Interfaces;
using Webefinity.Results;

namespace Webefinity.Module.Contact.Services
{

    public class StageContactService: IStageContactService
    {
        private readonly ContactDbContext dbContext;
        private readonly ILogger<StageContactService> logger;

        public StageContactService(ContactDbContext dbContext, ILogger<StageContactService> logger)
        {
            this.dbContext = dbContext;
            this.logger = logger;
        }

        public async Task<Result> StageContactAsync<TContactModel>(TContactModel model, string? type = null, int? contactLimit = null)
        {
            try
            {
                var contactCount = dbContext.Contacts.Count();
                if (contactLimit.HasValue && contactCount >= contactLimit.Value)
                {
                    logger.LogWarning("Contact limit reached, could not stage contact.");
                    return Result.Fail("Contact limit reached.", ResultReasonType.LimitReached);
                }

                var contactModelJson = JsonSerializer.Serialize(model);
                var contact = new ContactRecord { Data = contactModelJson, Type = type ?? typeof(TContactModel).FullName! };
                dbContext.Contacts.Add(contact);

                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Could not stage contact.");
                return Result.Fail(ex.Message);
            }

            return Result.Ok();
        }
    }
}
