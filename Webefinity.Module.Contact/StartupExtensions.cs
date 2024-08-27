using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.AspNetCore;
using Webefinity.Module.Contact.Data;
using Webefinity.Module.Contact.Interfaces;
using Webefinity.Module.Contact.Options;
using Webefinity.Module.Contact.Services;

namespace Webefinity.Module.Contact;

public static class StartupExtensions
{
    public static void AddWebefinityContact(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));

        var blogConnectionString = builder.Configuration.GetConnectionString("Contact");
        builder.Services.AddDbContextFactory<ContactDbContext>(options =>
        {
            options.UseSqlite(blogConnectionString ?? "Data Source=db/contact.db");
        });

        builder.Services.AddQuartz(q => {
            var jobKey = new JobKey("DailyContactsJob");
            q.AddJob<DailyContactsJob>(jobKey);
            q.AddTrigger(trigger =>
            {
                trigger.ForJob(jobKey);
                trigger.WithIdentity("DailyContactsTrigger");
                trigger.WithDailyTimeIntervalSchedule(1, IntervalUnit.Hour, daily => daily.OnMondayThroughFriday().StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(0, 0)).EndingDailyAfterCount(1));
                //trigger.StartNow();
            });
        });

        builder.Services.AddQuartzServer(options =>
        {
            options.WaitForJobsToComplete = true;
        });

        builder.Services.AddScoped<ISendContactService, SendContactService>();
        builder.Services.AddScoped<IStageContactService, StageContactService>();

        builder.Services.AddHostedService<ContactMigrateHostedService>();
    }
}
