using System;
using System.Net.Http.Json;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Webefinity.Module.Guides.Abstractions;

namespace Webefinity.Module.Guides.Services;

public abstract class GuideServiceBase
{
    protected abstract HttpClient HttpClient { get; }
    protected readonly IGuideAvailable? guideAvailableService;
    private bool isVisible;
    private bool? overrideHidden;
    private HashSet<Components.Guide> registeredGuides = new HashSet<Components.Guide>();
    private GuideIndex? index;

    protected GuideServiceBase(IServiceProvider serviceProvider)
    {
        this.guideAvailableService = serviceProvider.GetService<IGuideAvailable>();
        this.isVisible = true;
    }

    public bool IsVisible => isVisible;

    private async Task LoadIndexGuideAsync(CancellationToken cancellationToken)
    {
        if (this.index is null)
        {
            this.index = await this.HttpClient.GetFromJsonAsync<GuideIndex>("guide/index.json", cancellationToken);
            ArgumentNullException.ThrowIfNull(index, nameof(index));
        }
    }

    public async Task<GuideContentRecord> GetGuideContentAsync(string guideName, CancellationToken cancellationToken)
    {
        await LoadIndexGuideAsync(cancellationToken);

        var guideFound = index!.Guides.ContainsKey(guideName);
        if (!guideFound)
        {
            return await LoadNotFoundGuideAsync($"{guideName} not found.", cancellationToken);
        }

        var guide = index.Guides[guideName];

        var response = await HttpClient.GetAsync($"guide/{guide.Md}", cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return new(guideName, guide.Title, Markdig.Markdown.ToHtml(await response.Content.ReadAsStringAsync(cancellationToken)));
        }

        return await LoadErrorGuideAsync($"Could not load {guideName}", cancellationToken);
    }

    private async Task<GuideContentRecord> LoadNotFoundGuideAsync(string title, CancellationToken cancellationToken)
    {
        var response = await this.HttpClient.GetAsync("guide/notfound.md", cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return new("notfound", title, Markdig.Markdown.ToHtml(await response.Content.ReadAsStringAsync(cancellationToken)));
        }

        throw new Exception("Not found guide 'notfound.md' is missing.");
    }

    private async Task<GuideContentRecord> LoadErrorGuideAsync(string title, CancellationToken cancellationToken)
    {
        var response = await this.HttpClient.GetAsync("guide/error.md", cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return new("error", title, Markdig.Markdown.ToHtml(await response.Content.ReadAsStringAsync(cancellationToken)));
        }

        throw new Exception("Error guide 'error.md' is missing.");
    }

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

    public async Task SetVisibilityAsync(bool? value = null)
    {
        isVisible = value ?? !isVisible;
        if (isVisible)
        {
            await RefreshAsync();
        }
    }

    public async Task<bool> IsGuideHiddenAsync(CancellationToken cancellationToken)
    {
        if (overrideHidden.HasValue)
        {
            return overrideHidden.Value;
        }

        if (guideAvailableService is null)
        {
            return false;
        }

        return  await guideAvailableService.IsGuideHiddenAsync(cancellationToken);
    }

    public Task SetIsGuideHiddenAsync(bool hidden, CancellationToken cancellationToken)
    {
        if (guideAvailableService is null)
        {
            return Task.CompletedTask;
        }

        overrideHidden = null;
        return guideAvailableService.SetGuideHiddenAsync(hidden, cancellationToken);
    }

    public void RegisterGuide(Components.Guide guideInstance)
    {
        this.registeredGuides.Add(guideInstance);
    }

    public void UnregisterGuide(Components.Guide guideInstance)
    {
        this.registeredGuides.Remove(guideInstance);
    }


    public async Task RefreshAsync()
    {
        foreach (var guide in registeredGuides)
        {
            await guide.RefreshAsync();
        }
    }

    public async Task ReloadAsync(string guideName, CancellationToken cancellationToken = default)
    {
        foreach (var guide in registeredGuides)
        {
            await guide.ReloadAsync(guideName, cancellationToken);
        }
    }

    public async Task TransitionGuideAsync(string guideName, CancellationToken cancellationToken = default)
    {
        if (overrideHidden.HasValue && overrideHidden.Value)
        {
            return;
        }

        if (!isVisible)
        {
            return;
        }

        if (!await IsGuideAvailableAsync(cancellationToken))
        {
            return;
        }

        await ReloadAsync(guideName, cancellationToken);
    }

    public async Task HideGuideAsync(CancellationToken cancellationToken = default)
    {
        overrideHidden = true;
        await RefreshAsync();
    }

    public async Task ShowGuideAsync(string guideName, CancellationToken cancellationToken = default)
    {
        overrideHidden = false;
        await SetVisibilityAsync(true);
        await ReloadAsync(guideName, cancellationToken);
    }
}
