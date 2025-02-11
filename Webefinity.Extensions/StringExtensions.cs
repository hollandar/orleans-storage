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

    public static string Or(this string? subject, params IEnumerable<string?> def) {
        return string.IsNullOrWhiteSpace(subject) ? 
            def.FirstOrDefault(r => !String.IsNullOrWhiteSpace(r)) ?? throw new ArgumentException("No non-empty defaults.")
            : subject;
    }
}
