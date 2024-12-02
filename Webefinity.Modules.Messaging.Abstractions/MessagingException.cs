
namespace Webefinity.Module.Messaging.Abstractions;

[Serializable]
public class MessagingException : Exception
{
    public MessagingException()
    {
    }

    public MessagingException(string? message) : base(message)
    {
    }

    public MessagingException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}