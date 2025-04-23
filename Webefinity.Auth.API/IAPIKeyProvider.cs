namespace Webefinity.Auth.API;

public interface IAPIKeyProvider
{
    public string GetEndpoint();
    public string[] GetKeyStrings();
}
