using System;
using System.Collections.Generic;
using System.Text;

namespace Webefinity.Module.Blocks
{
    public static class UriTransformer
    {
        public static string TransformSize(string url, string size = "", string defaultUrl = "unspecified.jpg")
        {
            if (string.IsNullOrEmpty(url))
            {
                return defaultUrl;
            }
            return url;
        }

        public static string Transform(string url, string defaultUrl = "unspecified.jpg")
        {
            if (string.IsNullOrEmpty(url))
            {
                return defaultUrl;
            }
            return url;
        }
    }
}
