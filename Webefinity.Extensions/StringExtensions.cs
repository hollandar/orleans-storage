using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Webefinity.Extensions;

public static partial class StringExtensions
{
    public static string NumberOnly(this string input)
    {
        return new string(input.Where(char.IsDigit).ToArray());
    }

    [GeneratedRegex("^(?<email>[a-zA-Z0-9._%-]+)(?<extension>[+][a-zA-Z0-9._%+-]+){0,1}(?<domain>@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,})$")]
    private static partial Regex EmailExtensionRegex();

    public static string UnextendedEmailAddress(this string input)
    {
        var match = EmailExtensionRegex().Match(input);
        if (match.Success)
        {
            return $"{match.Groups["email"].Value}{match.Groups["domain"].Value}";
        }
        else
        {
            return input;
        }
    }

    public static string Or(this string? subject, params IEnumerable<string?> def)
    {
        return string.IsNullOrWhiteSpace(subject) ?
            def.FirstOrDefault(r => !String.IsNullOrWhiteSpace(r)) ?? throw new ArgumentException("No non-empty defaults.")
            : subject;
    }

    /// <summary>
    /// Formats a string by replacing placeholders with corresponding values from a dictionary.
    /// </summary>
    /// <param name="format">
    /// The format string containing placeholders enclosed in curly braces (e.g., {key}).
    /// Placeholders can also include optional left and right parts separated by '?' (e.g., {left?key?right}).
    /// </param>
    /// <param name="values">
    /// A dictionary containing key-value pairs where the key matches the placeholder in the format string,
    /// and the value is the replacement text.
    /// </param>
    /// <returns>
    /// A formatted string where placeholders are replaced with their corresponding values from the dictionary.
    /// If a placeholder is not found in the dictionary, it remains unchanged in the output.
    /// </returns>
    /// <exception cref="FormatException">
    /// Thrown if the format string contains invalid or mismatched placeholders.
    /// </exception>
    public static string ExtendedFormat(this string format, IDictionary<string, string?> values)
    {
        var stringBuilder = new StringBuilder(format.Length + values.Values.Sum(r => r?.Length ?? 0));
        var formatSpan = format.AsSpan();
        int startTagIndex = -1;
        List<int> tagIndices = new(3);
        for (int i = 0; i < formatSpan.Length; i++)
        {
            switch (formatSpan[i])
            {
                case '{' when startTagIndex == -1:
                    startTagIndex = i;
                    break;
                case '{' when startTagIndex != -1:
                    stringBuilder.Append(formatSpan.Slice(startTagIndex, i - startTagIndex));
                    startTagIndex = i;
                    break;
                case '}':
                    ReadOnlySpan<char> tag;
                    ReadOnlySpan<char> leftPart = ReadOnlySpan<char>.Empty;
                    ReadOnlySpan<char> rightPart = ReadOnlySpan<char>.Empty;
                    switch (tagIndices.Count)
                    {
                        case 0:
                            tag = formatSpan.Slice(startTagIndex + 1, i - startTagIndex - 1);
                            break;
                        case 1:
                            ReadOnlySpan<char> partA = formatSpan.Slice(startTagIndex + 1, tagIndices[0] - startTagIndex - 1);
                            ReadOnlySpan<char> partB = formatSpan.Slice(tagIndices[0] + 1, i - tagIndices[0] - 1);
                            if (values.ContainsKey(partA.ToString()))
                            {
                                tag = partA;
                                rightPart = partB;
                            }
                            else
                            {
                                leftPart = partA;
                                tag = partB;
                            }
                            break;
                        case 2:
                            tag = formatSpan.Slice(tagIndices[0] + 1, tagIndices[1] - tagIndices[0] - 1);
                            leftPart = formatSpan.Slice(startTagIndex + 1, tagIndices[0] - startTagIndex - 1);
                            rightPart = formatSpan.Slice(tagIndices[1] + 1, i - tagIndices[1] - 1);
                            break;
                        default:
                            throw new FormatException("Invalid format string: " + format);
                    }

                    ReadOnlySpan<char> value = ReadOnlySpan<char>.Empty;
                    var tagString = tag.ToString();
                    if (values.ContainsKey(tagString))
                    {
                        value = values[tagString] is not null ? values[tagString].AsSpan() : ReadOnlySpan<char>.Empty;
                    }
                    else
                    {
                        value = formatSpan.Slice(startTagIndex, i - startTagIndex + 1);
                    }

                    if (value.Length > 0)
                    {
                        stringBuilder.Append(leftPart);
                        stringBuilder.Append(value);
                        stringBuilder.Append(rightPart);
                    }

                    startTagIndex = -1;
                    tagIndices.Clear();
                    break;
                case var c when startTagIndex == -1:
                    stringBuilder.Append(c);
                    break;
                case var c when startTagIndex != -1 && c == '?':
                    tagIndices.Add(i);
                    break;
                case var c when startTagIndex != -1:
                    break;
            }
        }

        return stringBuilder.ToString();
    }

    public static HashSet<int> RangesToNumbers(this String input)
    {
        // Example: "1-3,5,7-9" => {1,2,3,5,7,8,9}
        var result = new HashSet<int>();
        var parts = input.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var part in parts)
        {
            var rangeParts = part.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (rangeParts.Length == 1 && int.TryParse(rangeParts[0], out int singleNumber))
            {
                result.Add(singleNumber);
            }
            else if (rangeParts.Length == 2
                && int.TryParse(rangeParts[0], out int start)
                && int.TryParse(rangeParts[1], out int end)
                && end >= start)
            {
                for (int i = start; i <= end; i++)
                {
                    result.Add(i);
                }
            }
        }
        return result;
    }
}
