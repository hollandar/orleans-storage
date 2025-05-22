using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Webefinity.Content.FileIndex.Services;
using Webefinity.ContentRoot.Abstractions;

namespace Webefinity.Content.FileIndex.Tests
{
    public class IndexDatabaseTests
    {
        IServiceProvider serviceProvider;
        public IndexDatabaseTests()
        {
            var services = new ServiceCollection();
            services.AddSingleton<FileIndexDatabaseService>();

            this.serviceProvider = services.BuildServiceProvider();
        }

        [Fact]
        public void WriteMetaString()
        {
            var lib = this.serviceProvider.GetRequiredService<FileIndexDatabaseService>();
            lib.ClearMeta(CollectionDef.DefaultCollection);

            var file = "test.txt";
            lib.WriteMeta(CollectionDef.DefaultCollection, file, "text", "test");
            var result = lib.ReadMeta<string>(CollectionDef.DefaultCollection, file, "text");

            Assert.Equal("test", result);
        }

        record TwoValues(string one, string two);
        [Fact]
        public void WriteMetaObject()
        {
            var lib = this.serviceProvider.GetRequiredService<FileIndexDatabaseService>();
            lib.ClearMeta(CollectionDef.DefaultCollection);

            var file = "test.txt";
            lib.WriteMeta(CollectionDef.DefaultCollection, file, "Two-Values", new TwoValues("1", "2"));
            var result = lib.ReadMeta<TwoValues>(CollectionDef.DefaultCollection, file, "Two-Values");

            Assert.NotNull(result);
            Assert.Equal("1", result.one);
            Assert.Equal("2", result.two);
        }

        [Fact]
        public void WriteMetaTwiceString()
        {
            var lib = this.serviceProvider.GetRequiredService<FileIndexDatabaseService>();
            lib.ClearMeta(CollectionDef.DefaultCollection);

            var file = "test.txt";
            lib.WriteMeta(CollectionDef.DefaultCollection, file, "text", "ORIGINAL");
            lib.WriteMeta(CollectionDef.DefaultCollection, file, "text", "NEW");
            var result = lib.ReadMeta<string>(CollectionDef.DefaultCollection, file, "text");

            Assert.Equal("NEW", result);
        }

        [Fact]
        public void ReadEmptyMeta()
        {
            var lib = this.serviceProvider.GetRequiredService<FileIndexDatabaseService>();
            lib.ClearMeta(CollectionDef.DefaultCollection);

            var file = "test.txt";
            var result = lib.ReadMeta<string>(CollectionDef.DefaultCollection, file, "text");

            Assert.Null(result);
        }

        [Fact]
        public void SerializeEmptyMeta()
        {
            var lib = this.serviceProvider.GetRequiredService<FileIndexDatabaseService>();
            lib.ClearMeta(CollectionDef.DefaultCollection);

            var result = lib.SerializeMeta(CollectionDef.DefaultCollection);
            var expected = """{"Collection":"Default","Files":[]}""";

            Assert.Equal(expected, result);
        }

        [Fact]
        public void SerializeSingleMeta()
        {
            var lib = this.serviceProvider.GetRequiredService<FileIndexDatabaseService>();
            lib.ClearMeta(CollectionDef.DefaultCollection);
            var file = "test.txt";
            lib.WriteMeta(CollectionDef.DefaultCollection, file, "text", "test");

            var result = lib.SerializeMeta(CollectionDef.DefaultCollection);
            var expected = """{"Collection":"Default","Files":[{"FileName":"test.txt","Meta":{"text":"test"}}]}""";

            Assert.Equal(expected, result);
        }

        [Fact]
        public void DeserializeSingleMeta()
        {
            var lib = this.serviceProvider.GetRequiredService<FileIndexDatabaseService>();
            lib.ClearMeta(CollectionDef.DefaultCollection);
            lib.DeserializeMeta("""{"Collection":"Default","Files":[{"FileName":"test.txt","Meta":{"text":"test"}}]}""");

            var file = "test.txt";
            var result = lib.ReadMeta<string>(CollectionDef.DefaultCollection, file, "text");

            Assert.Equal("test", result);
        }

        public record ImageSize(int width, int height);

        [Fact]
        public void RoundTrip()
        {
            var lib = this.serviceProvider.GetRequiredService<FileIndexDatabaseService>();
            lib.ClearMeta(CollectionDef.DefaultCollection);

            var file = "picture.jpg";
            lib.WriteMeta(CollectionDef.DefaultCollection, file, "Content-Type", "image/jpeg");
            lib.WriteMeta(CollectionDef.DefaultCollection, file, "Image-Size", new ImageSize(800, 600));
            lib.WriteMeta(CollectionDef.DefaultCollection, file, "Tags", new string[] { "image", "seaside" });
            var metaJson = lib.SerializeMeta(CollectionDef.DefaultCollection);

            lib.ClearMeta(CollectionDef.DefaultCollection);
            lib.DeserializeMeta(metaJson);

            var result = lib.ReadMeta<string>(CollectionDef.DefaultCollection, file, "Content-Type");
            Assert.Equal("image/jpeg", result);
            var result2 = lib.ReadMeta<ImageSize>(CollectionDef.DefaultCollection, file, "Image-Size");
            Assert.NotNull(result2);
            Assert.Equal(800, result2.width);
            Assert.Equal(600, result2.height);
            var result3 = lib.ReadMeta<string[]>(CollectionDef.DefaultCollection, file, "Tags");
            Assert.NotNull(result3);
            Assert.Equal(2, result3.Length);
            Assert.Equal("image", result3[0]);
            Assert.Equal("seaside", result3[1]);

        }

    }
}
