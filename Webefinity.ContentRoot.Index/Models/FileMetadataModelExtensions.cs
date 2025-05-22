using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Webefinity.ContentRoot.Index.Data;

namespace Webefinity.ContentRoot.Index.Models
{
    public static class FileMetadataModelExtensions
    {
        public static FileMetadataModel ToFileMetadataModel(this FileMetadata fileMetadata)
        {
            return new FileMetadataModel
            {
                Id = fileMetadata.Id,
                FileName = fileMetadata.FileName,
                Collection = fileMetadata.Collection,
                Metadata = fileMetadata.Metadata.Select(ToMetadataModel).ToList(),
            };
        }

        public static MetadataModel ToMetadataModel(this Metadata metadata)
        {
            return new MetadataModel
            {
                Id = metadata.Id,
                Key = metadata.Key,
                Value = metadata.Value,
            };
        }

        public static Metadata ToMetadata(this MetadataModel model)
        {
            return new Metadata
            {
                Id = Guid.CreateVersion7(),
                Key = model.Key,
                Value = model.Value,
            };
        }
    }
}
