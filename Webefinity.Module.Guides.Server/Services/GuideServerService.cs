using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Webefinity.Module.Guides.Abstractions;
using Webefinity.Module.Guides.Services;

namespace Webefinity.Module.Guides.Server.Services;

public class GuideServerService : IGuideService
{
    private readonly HttpContext httpContext;
    private readonly IGuideAvailable? guideAvailableService;
    private bool isVisible = true;
    private GuideIndex? index;
    private HashSet<Components.Guide> registeredGuides = new HashSet<Components.Guide>();

    public GuideServerService(IServiceProvider serviceProvider)
    {
        var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
        this.httpContext = httpContextAccessor.HttpContext ?? throw new InvalidOperationException("HttpContext is not available.");

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

    private async Task LoadIndexGuideAsync(CancellationToken cancellationToken)
    {
        if (this.index is null)
        {
            using var client = new HttpClient();
            var uri = new Uri($"{httpContext.Request.Scheme}://{httpContext.Request.Host}/{httpContext.Request.PathBase.ToString().TrimStart('/')}/guide/index.json");
            this.index = await client.GetFromJsonAsync<GuideIndex>(uri, cancellationToken);
            ArgumentNullException.ThrowIfNull(index, nameof(index));
        }
    }

    public async Task<string> GetGuideContentAsync(string guideName, CancellationToken cancellationToken)
    {
        await LoadIndexGuideAsync(cancellationToken);

        var guideFound = index!.Guides.ContainsKey(guideName);
        if (!guideFound)
        {
            return await LoadNotFoundGuideAsync(cancellationToken);
        }

        var guide = index.Guides[guideName];

        var uri = new Uri($"{httpContext.Request.Scheme}://{httpContext.Request.Host}/{httpContext.Request.PathBase.ToString().TrimStart('/')}/guide/{guide.Md}");
        using var client = new HttpClient();

        var response = await client.GetAsync(uri, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return Markdig.Markdown.ToHtml(await response.Content.ReadAsStringAsync(cancellationToken));
        }

        return await LoadErrorGuideAsync(cancellationToken);
    }

    private async Task<string> LoadNotFoundGuideAsync(CancellationToken cancellationToken)
    {
        using var httpClient = new HttpClient();
        var uri = new Uri($"{httpContext.Request.Scheme}://{httpContext.Request.Host}/{httpContext.Request.PathBase.ToString().TrimStart('/')}/guide/notfound.md");
        var response = await httpClient.GetAsync(uri, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return Markdig.Markdown.ToHtml(await response.Content.ReadAsStringAsync(cancellationToken));
        }

        throw new Exception("Not found guide 'notfound.md' is missing.");
    }

    private async Task<string> LoadErrorGuideAsync(CancellationToken cancellationToken)
    {
        using var httpClient = new HttpClient();
        var uri = new Uri($"{httpContext.Request.Scheme}://{httpContext.Request.Host}/{httpContext.Request.PathBase.ToString().TrimStart('/')}/guide/error.md");
        var response = await httpClient.GetAsync(uri, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return Markdig.Markdown.ToHtml(await response.Content.ReadAsStringAsync(cancellationToken));
        }

        throw new Exception("Error guide 'error.md' is missing.");
    }

    public void RegisterGuide(Components.Guide guideInstance)
    {
        this.registeredGuides.Add(guideInstance);
    }

    public void UnregisterGuide(Components.Guide guideInstance)
    {
        this.registeredGuides.Remove(guideInstance);
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

        return guideAvailableService.SetGuideHiddenAsync(hidden, cancellationToken);
    }

    public async Task RefreshAsync()
    {
        foreach (var guide in registeredGuides)
        {
            await guide.RefreshAsync();
        }
    }
}
