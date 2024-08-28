using Webefinity.Module.Contact.Models;
using Webefinity.Results;

namespace Webefinity.Module.Contact.Interfaces;

public interface ISendContactService
{
    Task<Result> SendAsync(MailMessageModel sendMessage);
}
