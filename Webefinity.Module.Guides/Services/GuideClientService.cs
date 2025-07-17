using System;
using System.IO.Pipes;
using System.Net.Http.Json;
using Webefinity.Module.Guides.Abstractions;

namespace Webefinity.Module.Guides.Services;

public class GuideClientService : IGuideService
{
    private readonly HttpClient httpClient;
    private bool isVisible;
    private Task<GuideIndex?>? indexTask;
    private HashSet<Components.Guide> registeredGuides = new HashSet<Components.Guide>();

    public GuideClientService(HttpClient httpClient)
    {
        this.httpClient = httpClient;
        this.isVisible = true;
        this.indexTask = httpClient.GetFromJsonAsync<GuideIndex>("guide/index.json");
    }

    public bool IsVisible => isVisible;

    public async Task<string> GetGuideContentAsync(string guideName)
    {
        ArgumentNullException.ThrowIfNull(this.indexTask, nameof(this.indexTask));
        var index = await indexTask;
        ArgumentNullException.ThrowIfNull(index, nameof(index));

        var guideFound = index.Guides.ContainsKey(guideName);
        if (!guideFound)
        {
            return await LoadNotFoundGuideAsync();
        }

        var guide = index.Guides[guideName];
        var response = await httpClient.GetAsync($"guide/{guideName}.md");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return Markdig.Markdown.ToHtml(content);
        }

        return await LoadErrorGuideAsync();

    }

    private async Task<string> LoadNotFoundGuideAsync()
    {
        var response = await httpClient.GetAsync("guide/notfound.md");
        if (response.IsSuccessStatusCode)
        {
            return Markdig.Markdown.ToHtml(await response.Content.ReadAsStringAsync());
        }

        throw new Exception("Not found guide 'notfound.md' is missing.");
    }

    private async Task<string> LoadErrorGuideAsync()
    {
        var response = await httpClient.GetAsync("guide/error.md");
        if (response.IsSuccessStatusCode)
        {
            return Markdig.Markdown.ToHtml(await response.Content.ReadAsStringAsync());
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
            foreach (var guide in registeredGuides)
            {
                await guide.RefreshAsync();
            }
        }
    }
}
