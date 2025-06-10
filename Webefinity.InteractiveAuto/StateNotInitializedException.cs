
namespace Webefinity.InteractiveAuto;

[Serializable]
internal class StateNotInitializedException : Exception
{
    public StateNotInitializedException()
    {
    }

    public StateNotInitializedException(string? message) : base(message)
    {
    }

    public StateNotInitializedException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}