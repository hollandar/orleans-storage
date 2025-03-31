using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.Text.Json;

namespace Webefinity.Module.Blocks.Components.Blocks;

public class BlockEditorComponentBase<TModelType> : ComponentBase where TModelType : class, new()
{
    [Parameter] public string Kind { get; set; } = string.Empty;
    [Parameter] public string Name { get; set; } = string.Empty;
    [Parameter] public string Description { get; set; } = string.Empty;
    [Parameter] public int Sequence { get; set; } = 0;
    [Parameter] public JsonDocument? Data { get; set; } = null;
    [Parameter] public EventCallback<JsonDocument> OnApplyDocument { get; set; }
    [Parameter] public EventCallback<JsonDocument> OnSaveDocument { get; set; }
    [Parameter] public EventCallback OnCancelDocument { get; set; }

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


    protected async Task ApplyModel()
    {
        ArgumentNullException.ThrowIfNull(this.Model, "Model is null on save.");
        var data = JsonSerializer.SerializeToDocument(Model);
        if (data is null)
        {
            throw new InvalidOperationException("Failed to serialize model data.");
        }

        await this.OnApplyDocument.InvokeAsync(data);
    }

    protected async Task SaveModel()
    {
        ArgumentNullException.ThrowIfNull(this.Model, "Model is null on save.");
        var data = JsonSerializer.SerializeToDocument(Model);
        if (data is null)
        {
            throw new InvalidOperationException("Failed to serialize model data.");
        }

        await this.OnSaveDocument.InvokeAsync(data);
    }

    protected async Task CancelModel()
    {
        await this.OnCancelDocument.InvokeAsync();
    }

}
