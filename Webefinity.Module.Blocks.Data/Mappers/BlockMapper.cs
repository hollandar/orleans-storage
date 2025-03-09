using Webefinity.Module.Blocks.Abstractions;
using Webefinity.Module.Blocks.Data.Entities;

namespace Webefinity.Module.Blocks.Data.Mappers
{
    public static class BlockMapper
    {
        public static BlockModel Map(Block block)
        {
            return new BlockModel
            {
                Id = block.Id,
                Sequence = block.Sequence,
                Kind = block.Kind,
                Data = block.Data
            };
        }
    }
}
