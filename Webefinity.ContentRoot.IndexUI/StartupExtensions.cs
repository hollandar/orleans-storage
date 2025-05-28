using Microsoft.Extensions.DependencyInjection;
using Webefinity.ContentRoot.IndexUI.Interfaces;
using Webefinity.ContentRoot.IndexUI.Services;

namespace Webefinity.ContentRoot.IndexUI
{
    public static class StartupExtensions
    {
        public static void AddFileBrowserClientService(this IServiceCollection services, string adminPolicy = "IC_Admin")
        {
            services.AddScoped<IFileBrowserService, FileBrowserClientService>();
        }
    }
}
