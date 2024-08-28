using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Webefinity.Frontmatter
{
    public enum FrontmatterFormat
    {
        String,
        Json,
        Yaml,
        JsonOrYaml
    }

    public class FrontmatterContent<TFrontmatterType>
    {
        public TFrontmatterType? Frontmatter { get; set; } = default(TFrontmatterType);
        public string Content { get; set; } = string.Empty;
    }

    public static partial class FrontmatterDefaults
    {
        [GeneratedRegex("^---$")]
        public static partial Regex FrontmatterSeparator();
    }

    public class FrontmatterLoaderOptions
    {
        public static FrontmatterLoaderOptions Default => new FrontmatterLoaderOptions();
        public Regex Separator { get; set; } = FrontmatterDefaults.FrontmatterSeparator();
        public FrontmatterFormat Format { get; set; } = FrontmatterFormat.JsonOrYaml;
    }

    public class FrontmatterLoader
    {
        private static (string a, string b) LoadSections(StreamReader reader)
        {
            const int bufferLength = 8192;
            var frontMatterStream = new MemoryStream();
            var contentStream = new MemoryStream();
            int startDivider = -1;
            int endDivider = -1;

            {
                using var frontmatterWriter = new StreamWriter(frontMatterStream, leaveOpen: true);
                using var contentWriter = new StreamWriter(contentStream, leaveOpen: true);
                var buffer = new char[bufferLength];
                int fileOffset = 0;
                bool dividerFound = false;

                int dividerCount = 0;

                var reset = () =>
                {
                    startDivider = -1;
                    endDivider = -1;
                    dividerCount = 0;
                };

                var read = reader.Read(buffer, 0, bufferLength);
                while (read > 0)
                {
                    if (!dividerFound)
                    {
                        for (int i = 0; i < read; i++)
                        {
                            if (buffer[i] == '-')
                            {
                                if (dividerCount == 0)
                                {
                                    startDivider = i + fileOffset;
                                    dividerCount++;
                                }

                                else if (dividerCount == 1)
                                {
                                    dividerCount++;
                                }

                                else if (dividerCount == 2)
                                {
                                    dividerCount++;
                                }

                                else reset();
                            }
                            else if (buffer[i] == '\r')
                            {
                                if (dividerCount == 3)
                                {
                                    dividerCount++;
                                }
                                else reset();
                            }
                            else if (buffer[i] == '\n')
                            {
                                if (dividerCount == 3 || dividerCount == 4)
                                {
                                    endDivider = i + fileOffset;
                                    break;
                                }
                                else reset();
                            }
                            else reset();
                        }
                    }

                    if (endDivider == -1 && !dividerFound)
                    {
                        var arraySegment = new ReadOnlySpan<char>(buffer, 0, read);
                        frontmatterWriter.Write(arraySegment);
                    }
                    else if (endDivider > -1 && !dividerFound)
                    {
                        var frontMatter = new ReadOnlySpan<char>(buffer, 0, endDivider - fileOffset);
                        frontmatterWriter.Write(frontMatter);

                        var content = new ReadOnlySpan<char>(buffer, endDivider - fileOffset, read - (endDivider - fileOffset));
                        contentWriter.Write(content);
                        dividerFound = true;
                    }
                    else if (dividerFound)
                    {
                        var arraySegment = new ReadOnlySpan<char>(buffer, 0, read);
                        contentWriter.Write(arraySegment);
                    }

                    fileOffset += read;
                    read = reader.Read(buffer, 0, bufferLength);
                }

            }
            {
                frontMatterStream.Seek(0, SeekOrigin.Begin);
                contentStream.Seek(0, SeekOrigin.Begin);

                using var frontmatterReader = new StreamReader(frontMatterStream);
                var frontmatterString = frontmatterReader.ReadToEnd()[..(startDivider)];

                using var contentReader = new StreamReader(contentStream);
                var contentString = contentReader.ReadToEnd()[1..];

                return (frontmatterString, contentString);
            }
        }

        public static async Task<FrontmatterContent<TFrontmatterType>> LoadAsync<TFrontmatterType>(StreamReader streamReader, FrontmatterLoaderOptions? loaderOptions = null)
        {
            var sectionsLoaded = LoadSections(streamReader);

            var frontmatterLoaderOptions = loaderOptions ?? FrontmatterLoaderOptions.Default;

            if (sectionsLoaded.b.Length == 0)
            {
                return new FrontmatterContent<TFrontmatterType> { Content = sectionsLoaded.a };
            }
            else
            {
                TFrontmatterType? frontmatter = LoadFrontmatter<TFrontmatterType>(frontmatterLoaderOptions, sectionsLoaded.a);
                return new FrontmatterContent<TFrontmatterType> { Frontmatter = frontmatter, Content = sectionsLoaded.b };
            }
        }

        private static TFrontmatterType? LoadFrontmatter<TFrontmatterType>(FrontmatterLoaderOptions options, string frontmatter)
        {
            if (String.IsNullOrWhiteSpace(frontmatter))
            {
                return default(TFrontmatterType);
            }

            switch (options.Format)
            {
                case FrontmatterFormat.String:
                    {
                        if (typeof(TFrontmatterType) == typeof(string))
                            return (TFrontmatterType)Convert.ChangeType(frontmatter, typeof(TFrontmatterType));
                        else
                            throw new NotImplementedException("Only string is supported.");
                    }
                case FrontmatterFormat.JsonOrYaml when frontmatter.TrimStart(' ').StartsWith('{'):
                case FrontmatterFormat.Json:
                    {
                        var frontmatterObject = JsonSerializer.Deserialize<TFrontmatterType>(frontmatter);
                        return frontmatterObject;
                    }
                case FrontmatterFormat.JsonOrYaml when !frontmatter.TrimStart(' ').StartsWith('{'):
                case FrontmatterFormat.Yaml:
                    {
                        var deserializer = new DeserializerBuilder()
                            .WithNamingConvention(CamelCaseNamingConvention.Instance)
                            .WithCaseInsensitivePropertyMatching()
                            .IgnoreUnmatchedProperties()
                        .Build();

                        var frontmatterObject = deserializer.Deserialize<TFrontmatterType>(frontmatter);

                        return frontmatterObject;
                    }
                default:
                    throw new NotImplementedException("Only string, json or yaml are supported.");
            }
        }

        private static string ConsolidateLines(IEnumerable<string> lines)
        {
            var builder = new StringBuilder();
            foreach (var line in lines)
            {
                builder.AppendLine(line);
            }

            return builder.ToString();
        }


    }
}
