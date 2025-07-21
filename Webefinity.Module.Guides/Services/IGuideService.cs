using System;
using System.IO.Pipelines;
using Webefinity.Module.Guides.Components;
using Markdig.Parsers;

namespace Webefinity.Module.Guides.Services;

public interface IGuideService
{
    bool IsVisible { get; }
    Task<string> GetGuideContentAsync(string guideName, CancellationToken cancellationToken);
    void RegisterGuide(Guide guideInstance);
    void UnregisterGuide(Guide guideInstance);
    Task ToggleVisibilityAsync();
    Task<bool> IsGuideAvailableAsync(CancellationToken cancellationToken);
    Task<bool> IsGuideHiddenAsync(CancellationToken cancellationToken);
    Task SetIsGuideHiddenAsync(bool hidden, CancellationToken cancellationToken);
    Task RefreshAsync();
}
