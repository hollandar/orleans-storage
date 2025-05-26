namespace Webefinity.ContentRoot.Index.Uri;

public static class UriTransformer
{
    public static string Transform(string uriString)
    {
        ArgumentNullException.ThrowIfNull(uriString, nameof(uriString));
        if (uriString.StartsWith("indexed://", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var uri = new IndexedSchemeUri(uriString);
                return uri.ToString("icr/{Key}/{Collection}/{Path?/}{File}");
            }
            catch (ArgumentException)
            {
                // No need to rethrow the exception, just return the original string
            }
        }

        return uriString;
    }
}
