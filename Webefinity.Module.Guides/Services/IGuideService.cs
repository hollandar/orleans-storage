using System;
using Webefinity.Module.Guides.Components;

namespace Webefinity.Module.Guides.Services;

public interface IGuideService
{
    bool IsVisible { get; }
    Task<string> GetGuideContentAsync(string guideName);
    void RegisterGuide(Guide guideInstance);
    void UnregisterGuide(Guide guideInstance);
    Task ToggleVisibilityAsync();
}
