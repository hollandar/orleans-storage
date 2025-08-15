namespace Webefinity.ContentRoot.Index.Shared.Uri;

public static class UriTransformer
{
    public static string Transform(string? uriString, string defaultUri = "")
    {
        if (uriString?.StartsWith("indexed://", StringComparison.OrdinalIgnoreCase) ?? false)
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

        if (!String.IsNullOrWhiteSpace(uriString))
            return uriString;

        return defaultUri;
    }
    
    public static string TransformSize(string? uriString, ImageSizeEnum imageSize, string defaultUri = "")
    {
        if (uriString?.StartsWith("indexed://", StringComparison.OrdinalIgnoreCase) ?? false)
        {
            try
            {
                var uri = new IndexedSchemeUri(uriString);
                return uri.ToString("icr/{Key}/{Collection}/s/{Size}/{Path?/}{File}", imageSize);
            }
            catch (ArgumentException)
            {
                // No need to rethrow the exception, just return the original string
            }
        }

        if (!String.IsNullOrWhiteSpace(uriString))
            return uriString;
        
        return defaultUri;
    }
}
