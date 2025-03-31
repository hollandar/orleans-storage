using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Webefinity.Module.Blocks.Abstractions;
using Webefinity.Module.Blocks.Components.Blocks;

namespace Webefinity.Module.Blocks.Services;

internal class BlockDetail
{
    public string Kind { get; init; }
    public string Name { get; init; }
    public string Description { get; init; }
    public Type Type { get; init; }
    public Type? EditorType { get; private set; } = null;

    public BlockDetail(string kind, string name, string description, Type type)
    {
        this.Kind = kind;
        this.Name = name;
        this.Description = description;
        this.Type = type;
    }

    public void SetEditorType(Type editorType)
    {
        this.EditorType = editorType;
    }

    public static bool operator ==(BlockDetail left, string right)
    {
        return left.Kind == right;
    }

    public static bool operator !=(BlockDetail left, string right)
    {
        return left.Kind != right;
    }

    public override int GetHashCode()
    {
        return this.Kind.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        return this.Kind.Equals(obj);
    }
}

internal class BlockProviderService
{
    private static List<Assembly> blockAssemblies = new();
    private static Dictionary<string, BlockDetail>? blockTypes = null;
    private ILogger<BlockProviderService>? logger;

    public BlockProviderService(IServiceProvider serviceProvider)
    {
        this.logger = serviceProvider.GetService<ILogger<BlockProviderService>>();
        if (blockTypes is null)
        {
            ProbeBlockTypes();
        }
    }

    public static void AddAssemblies(params Assembly[] assemblies)
    {
        blockAssemblies.AddRange(assemblies);
    }

    private void ProbeBlockTypes()
    {
        blockTypes = new Dictionary<string, BlockDetail>();
        foreach (var assembly in blockAssemblies)
        {
            foreach (var type in assembly.GetTypes())
            {
                var customAttributes = type.GetCustomAttributes(typeof(BlockAttribute), true);
                if (customAttributes.Length == 0) continue;

                if (customAttributes.Length > 1)
                {
                    throw new InvalidOperationException($"Block type {type.FullName} has more than one BlockAttribute");
                }

                var blockAttribute = customAttributes[0] as BlockAttribute;
                if (blockAttribute is null)
                {
                    throw new InvalidOperationException($"Block type {type.FullName} has a null BlockAttribute");
                }

                if (blockTypes.ContainsKey(blockAttribute.Kind))
                {
                    throw new InvalidOperationException($"Block type {type.FullName} has a duplicate kind {blockAttribute.Kind}");
                }

                var blockDetail = new BlockDetail(blockAttribute.Kind, blockAttribute.Name, blockAttribute.Description, type);
                blockTypes.Add(blockAttribute.Kind, blockDetail);
            }

            foreach (var type in assembly.GetTypes())
            {

                var customEditAttributes = type.GetCustomAttributes(typeof(BlockEditorAttribute), true);
                if (customEditAttributes.Length == 0) continue;

                if (customEditAttributes.Length > 1)
                {
                    throw new InvalidOperationException($"Block type {type.FullName} has more than one BlockEditorAttribute");
                }

                var blockEditAttribute = customEditAttributes[0] as BlockEditorAttribute;
                if (blockEditAttribute is null)
                {
                    throw new InvalidOperationException($"Block type {type.FullName} has a null BlockEditorAttribute");
                }

                var blockDetail = blockTypes[blockEditAttribute.Kind];
                if (blockDetail is null)
                {
                    throw new InvalidOperationException($"Block type {type.FullName} is for a block kind {blockEditAttribute.Kind} that does not exist.");
                }

                if (blockDetail.EditorType is not null)
                {
                    throw new InvalidOperationException($"Block type {type.FullName} has a duplicate editor kind {blockEditAttribute.Kind}");
                }

                blockDetail.SetEditorType(type);
            }
        }
    }

    public (bool block, bool editor) HasBlock(string kind)
    {
        if (blockTypes!.ContainsKey(kind))
        {
            var blockDetail = blockTypes[kind];
            return (blockDetail.Type is not null, blockDetail.EditorType is not null);
        }

        return (false, false);
    }

    public RenderFragment RenderFragment(BlockModel model)
    {
        if (!blockTypes!.ContainsKey(model.Kind))
        {
            throw new ArgumentException($"Block with a kind {model.Kind} not found");
        }

        var blockDetail = blockTypes[model.Kind];

        if (blockDetail.Kind != model.Kind)
        {
            throw new InvalidOperationException($"Block {model.Kind} does not match block detail kind {blockDetail.Kind}.");
        }

        return builder =>
        {
            builder.OpenComponent(0, blockDetail.Type);
            builder.AddComponentParameter(1, "Kind", model.Kind);
            builder.AddComponentParameter(2, "Name", blockDetail.Name);
            builder.AddComponentParameter(3, "Description", blockDetail.Description);
            builder.AddComponentParameter(4, "Data", model.Data);
            builder.AddComponentParameter(5, "Sequence", model.Sequence);
            builder.CloseComponent();
        };
    }

    public RenderFragment RenderEditorFragment(BlockModel model, EventCallback<JsonDocument> onApplyDocument, EventCallback<JsonDocument> onSaveDocument, EventCallback onCancelDocument)
    {
        if (!blockTypes!.ContainsKey(model.Kind))
        {
            throw new ArgumentException($"Block with a kind {model.Kind} not found");
        }

        var blockDetail = blockTypes[model.Kind];

        if (blockDetail.Kind != model.Kind)
        {
            throw new InvalidOperationException($"Block {model.Kind} does not match block detail kind {blockDetail.Kind}.");
        }
        ArgumentNullException.ThrowIfNull(blockDetail.EditorType, $"Block {model.Kind} does not have an editor type.");

        return builder =>
        {
            builder.OpenComponent(0, blockDetail.EditorType);
            builder.AddComponentParameter(1, "Kind", model.Kind);
            builder.AddComponentParameter(2, "Name", blockDetail.Name);
            builder.AddComponentParameter(3, "Description", blockDetail.Description);
            builder.AddComponentParameter(4, "Data", model.Data);
            builder.AddComponentParameter(5, "Sequence", model.Sequence);
            builder.AddComponentParameter(6, "OnApplyDocument", onApplyDocument);
            builder.AddComponentParameter(7, "OnSaveDocument", onSaveDocument);
            builder.AddComponentParameter(8, "OnCancelDocument", onCancelDocument);
            builder.CloseComponent();
        };
    }

    public record BlockDescription(string Name, string Description, string Kind);

    public IEnumerable<BlockDescription> GetBlockDescriptions()
    {
        Debug.Assert(blockTypes is not null && blockTypes.Any());
        return blockTypes.Values.Select(b => new BlockDescription(b.Name, b.Description, b.Kind)).OrderBy(r => r.Name);
    }
}
