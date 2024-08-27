namespace Webefinity.Module.Contact.Data;

public class ContactRecord
{
    public Guid Id { get; set; } = UUIDNext.Uuid.NewDatabaseFriendly(UUIDNext.Database.SQLite);
    public string Data { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;
}
