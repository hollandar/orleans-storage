namespace Webefinity.Auth.API;

public interface IApiKeyClientProvider
{
    HttpClient GetKeyClient();
}
