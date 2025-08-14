using System;
using Webefinity.ContentRoot.Index.Interfaces;
using Webefinity.ContentRoot.IndexUI.Interfaces;

namespace Webefinity.ContentRoot.IndexUI.Services;

public class FileBrowserPolicyService : IFileBrowserPolicyService
{
    private readonly string? browserPolicy;
    private readonly string adminPolicy;

    public FileBrowserPolicyService(string? browserPolicy, string adminPolicy)
    {
        this.adminPolicy = adminPolicy ?? throw new ArgumentNullException(nameof(adminPolicy));
        this.browserPolicy = browserPolicy;
    }

    public string GetAdminPolicy()
    {
        return adminPolicy;
    }

    public string? GetBrowserPolicy()
    {
        return browserPolicy;
    }
}
