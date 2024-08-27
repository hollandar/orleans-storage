namespace Webefinity.Module.Blog.Data;

public class BlogWord
{
    public Guid Id { get; set; } = UUIDNext.Uuid.NewDatabaseFriendly(UUIDNext.Database.SQLite);
    public string Word { get; set; } = string.Empty;
    public int Count { get; set; } = 1;
    public string ArticleId { get; set; } = string.Empty;
    public virtual BlogArticle? Article { get; set; }
}
