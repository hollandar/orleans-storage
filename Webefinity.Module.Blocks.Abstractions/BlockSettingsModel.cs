using System;

namespace Webefinity.Module.Blocks.Abstractions;

public class UpdateBlockSettingsRequest
{
    public Guid Id { get; set; } = Guid.Empty;
    public string Name { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
}
