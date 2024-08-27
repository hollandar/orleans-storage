namespace Webefinity.Module.Blog.Data;

public class BlogTag
{
    public Guid Id { get; set; } = UUIDNext.Uuid.NewDatabaseFriendly(UUIDNext.Database.SQLite);
    public string Tag { get; set; } = string.Empty;
    public string ArticleId { get; set; } = string.Empty;
    public virtual BlogArticle? Article { get; set; }
}
