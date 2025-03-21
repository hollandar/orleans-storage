using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Webefinity.Module.Blocks.Abstractions
{
    public class BlockModel
    {
        public Guid Id { get; set; } = Guid.Empty;
        public string Kind { get; set; } = "none";
        public JsonDocument Data { get; set; } = JsonDocument.Parse("{}");
        public int Sequence { get; set; } = 0;
    }
}
