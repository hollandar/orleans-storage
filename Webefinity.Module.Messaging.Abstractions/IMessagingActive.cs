namespace Webefinity.Module.Messaging.Abstractions;

public interface IMessagingActive
{
    Task<bool> IsMessagingAsync();
}
