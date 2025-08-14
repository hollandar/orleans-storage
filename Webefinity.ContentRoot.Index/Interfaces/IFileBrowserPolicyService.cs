using System;

namespace Webefinity.ContentRoot.Index.Interfaces;

public interface IFileBrowserPolicyService
{
    string? GetBrowserPolicy();
    string GetAdminPolicy();
}
