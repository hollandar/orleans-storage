using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Webefinity.Content.FileIndex.Models
{
    public class CollectionMeta
    {
        public string Collection { get; set; } = string.Empty;
        public List<FileMeta> Files { get; set; } = new();
    }

    public class FileMeta
    {
        public string FileName { get; set; } = string.Empty;
        public IDictionary<string, JsonDocument> Meta { get; set; } = new Dictionary<string, JsonDocument>();
    }
}
