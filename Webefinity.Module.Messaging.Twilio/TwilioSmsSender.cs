using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Twilio.Clients;
using Twilio.Http;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using Webefinity.Module.Messaging.Abstractions;
using Webefinity.Module.Messaging.Abstractions.Models;

namespace Webefinity.Module.Messaging.Twilio;

public class TwilioSmsSender : ISmsSender
{
    private readonly ILogger<TwilioSmsSender>? logger;
    private readonly SmsTwilioOptions twilioOptions;
    private readonly TwilioRestClient twilioRestClient;

    public TwilioSmsSender(IServiceProvider serviceProvider)
    {
        var optionsProvider = serviceProvider.GetService<ISmsTwilioOptionsProvider>();
        if (optionsProvider is null)
        {
            throw new InvalidOperationException("The Twilio SMS options provider is not registered");
        }

        this.logger = serviceProvider.GetService<ILogger<TwilioSmsSender>>();
        this.twilioOptions = optionsProvider.GetSmsOptions();

        var httpClient = new System.Net.Http.HttpClient();
        ArgumentNullException.ThrowIfNullOrWhiteSpace(this.twilioOptions.AccountSid, "Twilio SMS AccountId is not set.");
        ArgumentNullException.ThrowIfNullOrWhiteSpace(this.twilioOptions.AuthToken, "Twilio SMS AuthToken is not set.");
        ArgumentNullException.ThrowIfNullOrWhiteSpace(this.twilioOptions.FromNumber, "Twilio SMS FromNumber is not set.");

        this.twilioRestClient = new TwilioRestClient(this.twilioOptions.AccountSid, this.twilioOptions.AuthToken, httpClient: new SystemNetHttpClient(httpClient));
    }

    public Task SendAsync(SmsMessageModel smsMessage, CancellationToken ct)
    {
        foreach (var toNumber in smsMessage.To)
        {
            if (!String.IsNullOrWhiteSpace(toNumber))
            {
                var message = MessageResource.Create(
                    body: smsMessage.Message,
                    from: new PhoneNumber(this.twilioOptions.FromNumber),
                    to: new PhoneNumber(toNumber),
                    client: this.twilioRestClient
                );

                logger?.LogInformation("Twilio SMS: Send to {ToNumber} - {Message} with Sid {Sid}.", toNumber, smsMessage.Message, message.Sid);
            }
        }

        return Task.CompletedTask;
    }
}
