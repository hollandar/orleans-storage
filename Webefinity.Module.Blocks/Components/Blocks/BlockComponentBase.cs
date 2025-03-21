using Microsoft.AspNetCore.Components;
using System.Text.Json;

namespace Webefinity.Module.Blocks.Components.Blocks;

public class BlockComponentBase<TModelType>: ComponentBase where TModelType : class, new()
{
    [Parameter] public string Kind { get; set; } = string.Empty;
    [Parameter] public string Name { get; set; } = string.Empty;
    [Parameter] public string Description { get; set; } = string.Empty;
    [Parameter] public int Sequence { get; set; } = 0;
    [Parameter] public JsonDocument? Data { get; set; } = null;

    protected TModelType Model { get; set; } = default!;
    protected override void OnParametersSet()
    {
        if (this.Data is not null)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var model = JsonSerializer.Deserialize<TModelType>(this.Data, options);

            if (model is not null)
            {
                this.Model = model;
            }
            else
            {
                throw new InvalidOperationException($"Failed to deserialize data for block {this.Kind} with data {Data.RootElement.GetRawText()}.");
            }
        }

        base.OnParametersSet();
    }
}
