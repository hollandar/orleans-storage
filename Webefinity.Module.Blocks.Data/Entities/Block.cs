using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Webefinity.Module.Blocks.Abstractions;

namespace Webefinity.Module.Blocks.Data.Entities;

public class Block
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid PageId { get; set; } = Guid.Empty;
    public virtual Page? Page { get; set; }
    public string Kind { get; set; } = "none";
    public JsonDocument Data { get; set; } = JsonDocument.Parse("{}");
    public int Sequence { get; set; } = 0;
}
