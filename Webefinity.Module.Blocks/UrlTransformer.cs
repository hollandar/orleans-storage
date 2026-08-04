using System;
using System.Collections.Generic;
using System.Text;

namespace Webefinity.Module.Blocks
{
    public static class UriTransformer
    {
        public static string Transform(string url, params string[] processingInstructions)
        {
            if (processingInstructions.Any())
            {
                var separator = url.Contains("?") ? "&" : "?";

                return $"{url}{separator}p={String.Join(',', processingInstructions)}";
            }
            else
                return url;
        }

        static int[] defaultSizes = new int[] { 320, 480, 640, 960, 1280, 1600, 1920, 2560, 3840 };
        public static string TransformSrcSet(string url, Func<int, string>? processingInstructionFactory = null, int[]? sizes = null)
        {
            sizes ??= defaultSizes;
            processingInstructionFactory ??= size => $"bicubic_wh({size})";

            var srcSetBuilder = new StringBuilder();

            return sizes.Select(size => $"{Transform(url, processingInstructionFactory(size))} {size}w").Aggregate((a, b) => $"{a}, {b}");
        }
    }
}
