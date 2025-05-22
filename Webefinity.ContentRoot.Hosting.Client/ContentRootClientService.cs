using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Webefinity.ContentRoot.Hosting.Client
{
    public class ContentRootClientService : IContentRootService
    {
        private HttpClient httpClient;
        private readonly string? key;

        public ContentRootClientService(HttpClient httpClient, string? key = null)
        {
            this.httpClient = httpClient;
            this.key = key;
        }

        public async Task<ContentStream> GetStreamContentAsync(string collection, string file, CancellationToken ct)
        {
            string url;
            if (this.key is null)
            {
                url = $"/content/{collection}/{file}";
            }
            else
            {
                url = $"/content/{this.key}/{collection}/{file}";
            }
            var response = await httpClient.GetAsync(url, ct);
            if (response.IsSuccessStatusCode)
            {
                return new ContentStream
                {
                    Success = true,
                    Stream = response.Content.ReadAsStream(ct),
                    ContentType = response.Content.Headers.Contains("Content-Type") ? response.Content.Headers.GetValues("Content-Type").First() : "application/octet-stream",
                    StatusCode = response.StatusCode
                };

            }

            else

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new ContentStream
                {
                    Success = false,
                    State = ContentState.NotFound,
                    StatusCode = response.StatusCode
                };
            }

            else

            {
                return new ContentStream
                {
                    Success = false,
                    StatusCode = response.StatusCode
                };
            }
        }
    }
}
