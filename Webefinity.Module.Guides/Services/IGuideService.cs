using System;
using System.IO.Pipelines;
using Webefinity.Module.Guides.Components;
using Markdig.Parsers;
using Webefinity.Module.Guides.Abstractions;

namespace Webefinity.Module.Guides.Services;

public interface IGuideService
{
    bool IsVisible { get; }
    Task<GuideContentRecord> GetGuideContentAsync(string guideName, CancellationToken cancellationToken);
    void RegisterGuide(Guide guideInstance);
    void UnregisterGuide(Guide guideInstance);
    Task SetVisibilityAsync(bool? visible = null);
    Task<bool> IsGuideAvailableAsync(CancellationToken cancellationToken);
    Task<bool> IsGuideHiddenAsync(CancellationToken cancellationToken);
    Task SetIsGuideHiddenAsync(bool hidden, CancellationToken cancellationToken);
    Task RefreshAsync();
    Task ShowGuideAsync(string guideName, CancellationToken cancellationToken);
    Task TransitionGuideAsync(string guideName, CancellationToken cancellationToken = default);
}
