using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace Webefinity.Results.Helpers;

public static class Randomness
{
    public static string RandomString(int length, string? baseCharacters = null)
    {
        string chars = baseCharacters ?? "abcdefghijklmnopqrstuvwxyz0123456789";
        var builder = new StringBuilder();
        for (int i = 0; i < length; i++)
        {
            builder.Append(chars[RandomNumberGenerator.GetInt32(chars.Length)]);
        }

        Debug.Assert(builder.Length == length);
        return builder.ToString();
    }
}
