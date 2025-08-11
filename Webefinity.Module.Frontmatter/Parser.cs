using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Webefinity.Module.Frontmatter;

public static partial class FrontmatterParser
{
    public static (bool hasFrontmatter, TFrontmatterType? frontmatter, string content) Parse<TFrontmatterType>(string content) where TFrontmatterType : class, new()
    {
        var match = FrontmatterRegex().Match(content);
        if (!match.Success)
        {
            throw new ArgumentException("Content does not match the expected frontmatter format.", nameof(content));
        }

        var frontmatterGroup = match.Groups["frontmatter"];
        var contentGroup = match.Groups["content"];

        TFrontmatterType? frontmatter = Activator.CreateInstance<TFrontmatterType>();
        if (frontmatterGroup.Success && TryDeserializeJson(frontmatterGroup.Value, out TFrontmatterType? parsedFrontmatter))
        {
            frontmatter = parsedFrontmatter;
        }

        return (frontmatterGroup.Success && contentGroup.Success, frontmatter, contentGroup.Value.Trim());
    }

    [GeneratedRegex(@"^(?:(?<frontmatter>.*)\n[-]{3,}\n){0,1}(?<content>.*)$", RegexOptions.Singleline)]
    private static partial Regex FrontmatterRegex();

    private static bool TryDeserializeJson<T>(string json, out T? result)
    {
        try
        {
            result = JsonSerializer.Deserialize<T>(json);
            return result is not null;
        }
        catch
        {
            result = default;
            return false;
        }
    }

}
