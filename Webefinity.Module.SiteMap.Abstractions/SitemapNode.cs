namespace Webefinity.Module.SiteMap.Abstractions;

public record SitemapNode(string Url, DateTimeOffset LastModified, string ChangeFrequency, double Priority);