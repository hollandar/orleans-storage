using System;
using System.IO.Pipes;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Webefinity.Module.Guides.Abstractions;

namespace Webefinity.Module.Guides.Services;

public class GuideClientService : GuideServiceBase, IGuideService
{
    private HttpClient httpClient;
    
    public GuideClientService(HttpClient httpClient, IServiceProvider serviceProvider) : base(serviceProvider)
    {
        this.httpClient = httpClient;
    }

    protected override HttpClient HttpClient => this.httpClient;
}
