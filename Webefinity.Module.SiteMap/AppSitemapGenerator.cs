using System;
using System.Reflection;
using Microsoft.AspNetCore.SignalR;
using Webefinity.Module.SiteMap.Abstractions;

namespace Webefinity.Module.SiteMap;

public class AppSitemapGenerator : ISitemapGenerator
{
    private static DateTimeOffset startupTime = DateTimeOffset.UtcNow;
    private SiteMapSectionConfiguration siteMapConfiguration;

    public AppSitemapGenerator(SiteMapSectionConfiguration siteMapConfiguration)
    {
        if (siteMapConfiguration == null)
        {
            throw new ArgumentNullException(nameof(siteMapConfiguration));
        }

        this.siteMapConfiguration = siteMapConfiguration;
    }

    public SitemapSection GetSection()
    {
        return new SitemapSection(this.siteMapConfiguration.Name, this.siteMapConfiguration.LastModified);
    }

    public Task<IEnumerable<SitemapNode>> GetSectionContentAsync()
    {
        List<SitemapNode> nodes = new List<SitemapNode>();
        foreach (var node in this.siteMapConfiguration.Nodes)
        {
            nodes.Add(new SitemapNode(node.Url, node.LastModified, node.ChangeFrequency, node.Priority));
        }

        return Task.FromResult<IEnumerable<SitemapNode>>(nodes);
    }

    public DateTimeOffset BuildTime(Assembly assembly)
    {
        var location = assembly.Location;
        if (string.IsNullOrEmpty(location) || !File.Exists(location))
        {
            return startupTime;
        }

        var lastWriteTime = File.GetLastWriteTimeUtc(location);
        return lastWriteTime < startupTime ? lastWriteTime : startupTime;
    }
}

public class SiteMapConfiguration
{
    public DateTimeOffset LastModified { get; private set; } = DateTimeOffset.UtcNow;
    public List<SiteMapSectionConfiguration> Sections { get; init; } = new List<SiteMapSectionConfiguration>();

    public SiteMapSectionConfiguration AddSection(string name)
    {
        var section = new SiteMapSectionConfiguration { Name = name, AssemblyLastModified = LastModified };
        Sections.Add(section);
        return section;
    }

    public SiteMapConfiguration AddAssemblyBuildTime(Assembly assembly)
    {
        if (assembly == null)
        {
            throw new ArgumentNullException(nameof(assembly));
        }

        LastModified = File.GetLastWriteTimeUtc(assembly.Location);

        return this;
    }
}

public class SiteMapSectionConfiguration
{
    public string Name { get; init; } = string.Empty;
    public DateTimeOffset AssemblyLastModified { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset LastModified => this.Nodes.Min(n => (DateTimeOffset?)n.LastModified) ?? AssemblyLastModified;
    public List<SiteMapNodeConfiguration> Nodes { get; init; } = new List<SiteMapNodeConfiguration>();

    public SiteMapSectionConfiguration AddNode(string url, DateTimeOffset? lastModified = null, string changeFrequency = "weekly", double priority = 0.8)
    {
        var node = new SiteMapNodeConfiguration
        {
            Url = url,
            LastModified = lastModified ?? LastModified,
            ChangeFrequency = changeFrequency,
            Priority = priority
        };
        Nodes.Add(node);
    
        return this;
    }
}

public class SiteMapNodeConfiguration
{
    public string Url { get; init; } = string.Empty;
    public DateTimeOffset LastModified { get; init; }
    public string ChangeFrequency { get; init; } = "weekly";
    public double Priority { get; init; } = 0.8;
}
