using System.Net;

namespace Webefinity.ContentRoot.Hosting.Client
{
    public enum ContentState
    {
        None,
        Found,
        NotFound,
    }

    public class ContentStream
    {
        public bool Success { get; set; } = false;
        public ContentState State { get; set; } = ContentState.None;
        public Stream? Stream { get; set; }
        public string? ContentType { get; set; }
        public HttpStatusCode StatusCode { get; set; }
    }

    public interface IContentRootService
    {
        Task<ContentStream> GetStreamContentAsync(string collection, string file, CancellationToken ct);
    }
}
