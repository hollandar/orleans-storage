namespace Webefinity.Module.Guides.Abstractions;

public record Version(int Major, int Minor);

public record Guide(string Title, string Md, int Order);

public class GuideIndex
{
    public Version Version { get; set; } = new Version(0, 1);
    public IDictionary<string, Guide> Guides { get; set; } = new Dictionary<string, Guide>();
}


