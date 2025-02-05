
namespace Webefinity.ContentRoot.Abstractions;

[Serializable]
public class LibraryPathNotFoundException : Exception
{
    public string? Path { get; init; }
    public LibraryPathNotFoundException(string? path = null)
    {
        this.Path = path;
    }

    public LibraryPathNotFoundException(string? message, string path) : base(message)
    {
        this.Path = path;
    }

    public LibraryPathNotFoundException(string? message, Exception? innerException, string path) : base(message, innerException)
    {
        Path = path;
    }
}