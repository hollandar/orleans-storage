using Webefinity.ContentRoot.Abstractions;
using Webefinity.ContentRoot.Hosting.Client;

namespace Webefinity.ContentRoot.Hosting
{
    public class ContentRootService: IContentRootService
    {
        private readonly IContentRootLibrary contentRootLibrary;

        public ContentRootService(IContentRootLibrary contentRootLibrary)
        {
            this.contentRootLibrary = contentRootLibrary;
        }

        public Task<ContentStream> GetStreamContentAsync(string collection, string file, CancellationToken ct)
        {
            ContentStream contentStream;
            var collectionDef = new CollectionDef(collection);
            if (contentRootLibrary.FileExists(collectionDef, file))
            {
                contentStream = new ContentStream
                {
                    Success = true,
                    Stream = contentRootLibrary.LoadReadStream(collectionDef, file),
                    ContentType = ContentTypes.GetContentType(file),
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }
            else
            {
                contentStream = new ContentStream
                {
                    Success = false,
                    State = ContentState.NotFound,
                    StatusCode = System.Net.HttpStatusCode.NotFound
                };
            }

            return Task.FromResult(contentStream);
        }
    }
}
