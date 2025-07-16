using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Webefinity.Module.Blocks.Abstractions;

namespace Webefinity.Module.Blocks.Data.Entities;

public class Page
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string Name { get; set; } = string.Empty;
    public virtual ICollection<Block> Blocks { get; set; } = new List<Block>();
    public string Title { get; set; } = string.Empty;
    public PublishState State { get; set; } = PublishState.Draft;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
