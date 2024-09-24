using System.Text.RegularExpressions;

namespace Webefinity.Validation;

public static partial class EmailAddress
{
    [GeneratedRegex("^(?<email>[a-zA-Z0-9._%-]+)(?<extension>[+][a-zA-Z0-9._%+-]+){0,1}(?<domain>@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,})$")]
    private static partial Regex EmailExtensionRegex();

    public static bool IsValidEmailAddress(this string? input, bool isRequired = false)
    {
        return (isRequired == false && String.IsNullOrEmpty(input)) || ( input is not null && EmailExtensionRegex().IsMatch(input));
    }

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
}
