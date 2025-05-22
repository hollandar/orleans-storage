using System.Text.Json;

namespace Webefinity.ContentRoot.Index.Models;

public class FileMetadataModel
{
    public FileMetadataModel()
    {
        
    }

    public Guid Id { get; set; } = Guid.Empty;
    public string Collection { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public List<MetadataModel> Metadata { get; set; } = new();
    public TType? GetMetadataValue<TType>(string key)
    {
        var metadata = this.Metadata.Where(r => r.Key == key).SingleOrDefault();
        if (metadata is null)
        {
            return default(TType);
        } else
        {
            var value = metadata.Value.Deserialize<TType>();
            return value;
        }
    }

    

}

public class MetadataModel
{
    public MetadataModel()
    {

    }

    public Guid Id { get; set; } = Guid.Empty;
    public string Key { get; set; } = string.Empty;
    public JsonDocument Value { get; set; } = JsonDocument.Parse("{}");

    

    public static MetadataModel Build<TType>(string key, TType value)
    {
        var documentValue = JsonSerializer.SerializeToDocument(value);
        return new MetadataModel
        {
            Key = key,
            Value = documentValue,
        };
    }
}
