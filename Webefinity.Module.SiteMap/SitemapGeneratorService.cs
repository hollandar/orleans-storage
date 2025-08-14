using System;
using System.Globalization;
using System.Xml;
using Webefinity.Module.SiteMap.Abstractions;

namespace Webefinity.Module.SiteMap;

public class SitemapGeneratorService
{
    private readonly IEnumerable<ISitemapGenerator> sitemapGenerators;

    public SitemapGeneratorService(IEnumerable<ISitemapGenerator> sitemapGenerators)
    {
        this.sitemapGenerators = sitemapGenerators ?? throw new ArgumentNullException(nameof(sitemapGenerators));
    }
    public XmlDocument GenerateRootSitemap(string baseUrl)
    {
        var xmlDoc = new XmlDocument();
        var root = xmlDoc.CreateElement("sitemapindex");
        root.SetAttribute("xmlns", "http://www.sitemaps.org/schemas/sitemap/0.9");
        xmlDoc.AppendChild(root);

        foreach (var generator in sitemapGenerators)
        {
            var section = generator.GetSection();
            var sitemapElement = xmlDoc.CreateElement("sitemap");

            var locElement = xmlDoc.CreateElement("loc");
            locElement.InnerText = $"{baseUrl.TrimEnd('/')}/{section.Name}/sitemap.xml";
            sitemapElement.AppendChild(locElement);

            var lastModifiedElement = xmlDoc.CreateElement("lastmod");
            lastModifiedElement.InnerText = section.LastModified.ToString("yyyy-MM-dd");
            sitemapElement.AppendChild(lastModifiedElement);

            root.AppendChild(sitemapElement);
        }

        return xmlDoc;
    }

    public async Task<XmlDocument> GenerateSectionSitemap(string baseUrl, string section)
    {
        var sitemapGenerator = sitemapGenerators.Where(r => r.GetSection().Name == section).FirstOrDefault();
        if (sitemapGenerator == null)
        {
            throw new InvalidOperationException($"Sitemap generator for section '{section}' not found.");
        }

        var xmlDoc = new XmlDocument();
        var root = xmlDoc.CreateElement("urlset");
        root.SetAttribute("xmlns", "http://www.sitemaps.org/schemas/sitemap/0.9");
        xmlDoc.AppendChild(root);

        var nodes = await sitemapGenerator.GetSectionContentAsync();
        foreach (var node in nodes)
        {
            var urlElement = xmlDoc.CreateElement("url");

            var locElement = xmlDoc.CreateElement("loc");
            locElement.InnerText = $"{baseUrl.TrimEnd('/')}/{node.Url.TrimStart('/')}";
            urlElement.AppendChild(locElement);

            var lastModifiedElement = xmlDoc.CreateElement("lastmod");
            lastModifiedElement.InnerText = node.LastModified.ToString("yyyy-MM-dd");
            urlElement.AppendChild(lastModifiedElement);

            if (!string.IsNullOrEmpty(node.ChangeFrequency))
            {
                var changeFreqElement = xmlDoc.CreateElement("changefreq");
                changeFreqElement.InnerText = node.ChangeFrequency;
                urlElement.AppendChild(changeFreqElement);
            }

            if (node.Priority > 0)
            {
                var priorityElement = xmlDoc.CreateElement("priority");
                priorityElement.InnerText = node.Priority.ToString("F1", CultureInfo.InvariantCulture);
                urlElement.AppendChild(priorityElement);
            }

            root.AppendChild(urlElement);
        }

        return xmlDoc;
    }
}
