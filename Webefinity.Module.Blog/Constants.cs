using Webefinity.ContentRoot;
using Webefinity.ContentRoot.Abstractions;

namespace Webefinity.Module.Blog;

public static class Constants
{
    public const string BlobReindexTrigger = "ArticleIndexer";
    public static CollectionDef BlogCollection = new CollectionDef("blog");
}
