using System.Text.RegularExpressions;
using Webefinity.Extensions;

namespace Webefinity.ContentRoot.Index;

public partial class IndexedSchemeUri
{

    public IndexedSchemeUri(string uriString)
    {
        ArgumentNullException.ThrowIfNull(uriString, nameof(uriString));
        var match = IndexSchemeRegex().Match(uriString);
        if (!match.Success)
        {
            throw new ArgumentException("Invalid URI format", nameof(uriString));
        }

        this.Key = match.Groups["key"].Value;
        this.Collection = match.Groups["collection"].Value;
        this.Path = match.Groups["path"].Success ? match.Groups["path"].Value : null;
        this.FileName = match.Groups["file"].Value;
    }

    public string Key { get; set; } = string.Empty;
    public string Collection { get; set; } = string.Empty;
    public string? Path { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;

    [GeneratedRegex(@"^indexed:\/\/(?<key>.*?)\/(?<collection>.*?)(?:\/(?<path>.*))?\/(?<file>.*?)$")]
    private static partial Regex IndexSchemeRegex();

    public override string ToString()
    {
        return "{Key}/{Collection}/{Path?/}{File}".ExtendedFormat(new Dictionary<string, string?>
        {
            { "Path", this.Path },
            { "Key", this.Key },
            { "Collection", this.Collection },
            { "File", this.FileName }
        });
    }

    public string ToString(string format)
    {
        ArgumentNullException.ThrowIfNull(format, nameof(format));
        return format.ExtendedFormat(new Dictionary<string, string?>
        {
            { "Path", this.Path },
            { "Key", this.Key },
            { "Collection", this.Collection },
            { "File", this.FileName }
        });
    }


}