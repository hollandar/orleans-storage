namespace Webefinity.Modules.Messaging.Abstractions;

public interface IMessagingActive
{
    Task<bool> IsMessagingAsync();
}
