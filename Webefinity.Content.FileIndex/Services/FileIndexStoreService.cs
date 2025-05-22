using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Webefinity.ContentRoot.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;

namespace Webefinity.Content.FileIndex.Services
{
    public class FileIndexStoreFactory
    {
        private readonly IServiceProvider serviceProvider;

        public FileIndexStoreFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }
        public async Task<IContentRootIndexLibrary> CreateAsync(string? key = null)
        {
            IContentRootIndexLibrary lib;
            if (key is null)
            {
                lib = new FileIndexStoreService(serviceProvider.GetRequiredService<IContentRootLibrary>(), serviceProvider.GetRequiredService<IMemoryCache>());
            } else
            {
                lib = new FileIndexStoreService(serviceProvider.GetRequiredKeyedService<IContentRootLibrary>(key), serviceProvider.GetRequiredService<IMemoryCache>());
            }

            await lib.InitializeAsync();
            return lib;
        }
    }

    public class FileIndexStoreService : IContentRootIndexLibrary, IContentRootLibrary
    {
        private readonly IContentRootLibrary contentRootLibrary;
        private readonly IMemoryCache memoryCache;

        public FileIndexStoreService(IContentRootLibrary contentRootLibrary, IMemoryCache memoryCache)
        {
            this.contentRootLibrary = contentRootLibrary;
            this.memoryCache = memoryCache;
        }

        
        public async Task InitializeAsync()
        {
        }

        // IContentRootLibrary delegated (mostly) to underlying implementation

        public bool DirectoryExists(CollectionDef collection, string directory)
        {
            return this.contentRootLibrary.DirectoryExists(collection, directory);
        }

        public Task<bool> DirectoryExistsAsync(CollectionDef collection, string directory)
        {
            return this.DirectoryExistsAsync(collection, directory);
        }

        public IAsyncEnumerable<string> EnumerateAsync(CollectionDef collection, string glob, string? insidePath = null)
        {
            return this.contentRootLibrary.EnumerateAsync(collection, glob, insidePath);
        }

        public IAsyncEnumerable<string> EnumerateRecursiveAsync(CollectionDef collection, string glob, string? insidePath = null)
        {
            return this.contentRootLibrary.EnumerateRecursiveAsync(collection, glob, insidePath);
        }

        public bool FileExists(CollectionDef collection, string file)
        {
            return this.contentRootLibrary.FileExists(collection, file);
        }

        public Task<bool> FileExistsAsync(CollectionDef collection, string file)
        {
            return this.FileExistsAsync(collection, file);
        }

        public string Load(CollectionDef collection, string file)
        {
            return this.contentRootLibrary.Load(collection, file);
        }

        public Task<string> LoadAsync(CollectionDef collection, string file)
        {
            return this.contentRootLibrary.LoadAsync(collection, file);
        }

        public T LoadJson<T>(CollectionDef collection, string file)
        {
            return this.contentRootLibrary.LoadJson<T>(collection, file);
        }

        public Task<T> LoadJsonAsync<T>(CollectionDef collection, string file)
        {
            return this.contentRootLibrary.LoadJsonAsync<T>(collection, file);
        }

        public StreamReader LoadReader(CollectionDef collection, string file)
        {
            return this.contentRootLibrary.LoadReader(collection, file);
        }

        public Task<StreamReader> LoadReaderAsync(CollectionDef collection, string file)
        {
            return this.contentRootLibrary.LoadReaderAsync(collection, file);
        }

        public Stream LoadReadStream(CollectionDef collection, string file)
        {
            return this.contentRootLibrary.LoadReadStream(collection, file);
        }

        public Task<Stream> LoadReadStreamAsync(CollectionDef collection, string file)
        {
            return this.contentRootLibrary.LoadReadStreamAsync(collection, file);
        }

        public Task RemoveAsync(CollectionDef collection, string file)
        {
            return this.contentRootLibrary.RemoveAsync(collection, file);
        }

        public Task SaveAsync(CollectionDef collection, string file, Stream content, string? contentType = null)
        {
            return this.contentRootLibrary.SaveAsync(collection, file, content, contentType);
        }
    }
}
