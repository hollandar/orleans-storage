using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webefinity.Auth.API
{
    internal class HttpClientProviderService: IApiKeyClientProvider
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IAPIKeyProvider apiKeyProvider;
        private readonly string header;

        public HttpClientProviderService(IHttpClientFactory httpClientFactory, IAPIKeyProvider apiKeyProvider, string header)
        {
            this.httpClientFactory = httpClientFactory;
            this.apiKeyProvider = apiKeyProvider;
            this.header = header;
        }
        public HttpClient GetKeyClient()
        {
            var keys = apiKeyProvider.GetKeyStrings();
            var random = Random.Shared.Next(0, keys.Length);
            var httpClient = httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri(apiKeyProvider.GetEndpoint());
            httpClient.DefaultRequestHeaders.Add(header, keys[random]);
            return httpClient;
        }
    }
}
