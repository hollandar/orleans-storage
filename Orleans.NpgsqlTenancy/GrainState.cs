using System.Text.Json;

namespace Orleans.NpgsqlTenancy;

public class GrainStateType: IDisposable
{
    public string StateName { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
    public JsonDocument State { get; set; } = JsonDocument.Parse("{}");
    public string ETag { get; set; } = Guid.NewGuid().ToString();

    public void Dispose() => State?.Dispose();
}
