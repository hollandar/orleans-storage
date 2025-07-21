using System;
using System.IO.Pipes;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Webefinity.Module.Guides.Abstractions;

namespace Webefinity.Module.Guides.Services;

public class GuideClientService : IGuideService
{
    private readonly HttpClient httpClient;
    private bool isVisible;
    private Task<GuideIndex?>? indexTask;
    private HashSet<Components.Guide> registeredGuides = new HashSet<Components.Guide>();
    private IGuideAvailable? guideAvailableService;
    public GuideClientService(HttpClient httpClient, IServiceProvider serviceProvider)
    {
        this.httpClient = httpClient;
        this.isVisible = true;
        this.indexTask = httpClient.GetFromJsonAsync<GuideIndex>("guide/index.json");

        this.guideAvailableService = serviceProvider.GetService<IGuideAvailable>();
    }

    public bool IsVisible => isVisible;
    public async Task<bool> IsGuideAvailableAsync(CancellationToken cancellationToken)
    {
        if (guideAvailableService is null)
        {
            return true;
        }
        else
        {
            return await guideAvailableService.IsGuideAvailableAsync(cancellationToken);
        }
    }

    public async Task<string> GetGuideContentAsync(string guideName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(this.indexTask, nameof(this.indexTask));
        var index = await indexTask;
        ArgumentNullException.ThrowIfNull(index, nameof(index));

        var guideFound = index.Guides.ContainsKey(guideName);
        if (!guideFound)
        {
            return await LoadNotFoundGuideAsync(cancellationToken);
        }

        var guide = index.Guides[guideName];
        var response = await httpClient.GetAsync($"guide/{guideName}.md", cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return Markdig.Markdown.ToHtml(content);
        }

        return await LoadErrorGuideAsync(cancellationToken);

    }

    private async Task<string> LoadNotFoundGuideAsync(CancellationToken cancellationToken)
    {
        var response = await httpClient.GetAsync("guide/notfound.md", cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return Markdig.Markdown.ToHtml(await response.Content.ReadAsStringAsync(cancellationToken));
        }

        throw new Exception("Not found guide 'notfound.md' is missing.");
    }

    private async Task<string> LoadErrorGuideAsync(CancellationToken cancellationToken)
    {
        var response = await httpClient.GetAsync("guide/error.md", cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return Markdig.Markdown.ToHtml(await response.Content.ReadAsStringAsync(cancellationToken));
        }

        throw new Exception("Error guide 'error.md' is missing.");
    }

    public void RegisterGuide(Components.Guide guideInstance)
    {
        registeredGuides.Add(guideInstance);
    }

    public void UnregisterGuide(Components.Guide guideInstance)
    {
        registeredGuides.Remove(guideInstance);
    }

    public async Task ToggleVisibilityAsync()
    {
        isVisible = !isVisible;
        if (isVisible)
        {
            await RefreshAsync();
        }
    }

    public Task<bool> IsGuideHiddenAsync(CancellationToken cancellationToken)
    {
        if (guideAvailableService is null)
        {
            return Task.FromResult(false);
        }
        return guideAvailableService.IsGuideHiddenAsync(cancellationToken);
    }

    public Task SetIsGuideHiddenAsync(bool hidden, CancellationToken cancellationToken)
    {
        if (guideAvailableService is null)
        {
            return Task.CompletedTask;
        }
        else
        {
            return guideAvailableService.SetGuideHiddenAsync(hidden, cancellationToken);
        }
    }

    public async Task RefreshAsync()
    {
        foreach (var guide in registeredGuides)
        {
            await guide.RefreshAsync();
        }
    }
}
