using Webefinity.Module.Blog.Data;

namespace Webefinity.Module.Blog.Components
{
    public static class ArticleStatusBadgeStateHelper
    {
        public static string GetBadgeState(ArticleState state)
        {
            return state switch
            {
                ArticleState.Draft => "badge badge-warning",
                ArticleState.Published => "badge badge-success",
                ArticleState.Archived => "badge",
                _ => "badge"
            };
        }
    }
}
