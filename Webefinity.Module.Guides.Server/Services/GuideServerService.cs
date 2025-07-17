using System;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Webefinity.Module.Guides.Abstractions;
using Webefinity.Module.Guides.Services;

namespace Webefinity.Module.Guides.Server.Services;

public class GuideServerService : IGuideService
{
    private readonly HttpContext httpContext;
    private bool isVisible;
    private GuideIndex? index;
    private HashSet<Components.Guide> registeredGuides = new HashSet<Components.Guide>();

    public GuideServerService(IHttpContextAccessor httpContextAccessor)
    {
        this.httpContext = httpContextAccessor.HttpContext ?? throw new ArgumentNullException(nameof(httpContextAccessor.HttpContext));
        isVisible = true;
    }

    public bool IsVisible => isVisible;

    private async Task LoadIndexGuideAsync()
    {
        if (this.index is null)
        {
            using var client = new HttpClient();
            var uri = new Uri($"{httpContext.Request.Scheme}://{httpContext.Request.Host}/{httpContext.Request.PathBase.ToString().TrimStart('/')}/guide/index.json");
            this.index = await client.GetFromJsonAsync<GuideIndex>(uri);
            ArgumentNullException.ThrowIfNull(index, nameof(index));
        }
    }

    public async Task<string> GetGuideContentAsync(string guideName)
    {
        await LoadIndexGuideAsync();

        var guideFound = index.Guides.ContainsKey(guideName);
        if (!guideFound)
        {
            return await LoadNotFoundGuideAsync();
        }

        var guide = index.Guides[guideName];

        var uri = new Uri($"{httpContext.Request.Scheme}://{httpContext.Request.Host}/{httpContext.Request.PathBase.ToString().TrimStart('/')}/guide/{guide.Md}");
        using var client = new HttpClient();

        var response = await client.GetAsync(uri);
        if (response.IsSuccessStatusCode)
        {
            return Markdig.Markdown.ToHtml(await response.Content.ReadAsStringAsync());
        }

        return await LoadErrorGuideAsync();
    }

    private async Task<string> LoadNotFoundGuideAsync()
    {
        using var httpClient = new HttpClient();
        var uri = new Uri($"{httpContext.Request.Scheme}://{httpContext.Request.Host}/{httpContext.Request.PathBase.ToString().TrimStart('/')}/guide/notfound.md");
        var response = await httpClient.GetAsync(uri);
        if (response.IsSuccessStatusCode)
        {
            return Markdig.Markdown.ToHtml(await response.Content.ReadAsStringAsync());
        }

        throw new Exception("Not found guide 'notfound.md' is missing.");
    }

    private async Task<string> LoadErrorGuideAsync()
    {
        using var httpClient = new HttpClient();
        var uri = new Uri($"{httpContext.Request.Scheme}://{httpContext.Request.Host}/{httpContext.Request.PathBase.ToString().TrimStart('/')}/guide/error.md");
        var response = await httpClient.GetAsync(uri);
        if (response.IsSuccessStatusCode)
        {
            return Markdig.Markdown.ToHtml(await response.Content.ReadAsStringAsync());
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
            foreach (var guide in registeredGuides)
            {
                await guide.RefreshAsync();
            }
        }
    }
}
