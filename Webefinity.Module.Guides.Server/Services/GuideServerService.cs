using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Webefinity.Module.Guides.Abstractions;
using Webefinity.Module.Guides.Services;

namespace Webefinity.Module.Guides.Server.Services;

public class GuideServerService : GuideServiceBase, IGuideService
{
    private readonly HttpClient httpClient;
    
    public GuideServerService(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
        var httpContext = httpContextAccessor.HttpContext ?? throw new InvalidOperationException("HttpContext is not available.");
        this.httpClient = new HttpClient
        {
            BaseAddress = new Uri($"{httpContext.Request.Scheme}://{httpContext.Request.Host}/{httpContext.Request.PathBase.ToString().TrimStart('/')}/")
        };
    }

    protected override HttpClient HttpClient => this.httpClient;


   

}
