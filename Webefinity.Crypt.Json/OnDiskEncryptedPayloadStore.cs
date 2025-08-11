using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Webefinity.Crypt.Json
{

    public class OnDiskEncryptedPayloadStore : IEncryptedPayloadStore
    {
        private readonly ReaderWriterLockSlim semaphoreSlim = new ReaderWriterLockSlim();
        private readonly string path;

        public OnDiskEncryptedPayloadStore(string path)
        {
            this.path = path;
            if (!Directory.Exists(path))
            {
                throw new ArgumentException($"Path {path} is not valid.");
            }

        }

        private string GetFileName(string key) => $"{key}.epl";

        public Task SetEncryptedPayloadAsync(string key, EncryptedPayload payload)
        {
            semaphoreSlim.EnterWriteLock();
            try
            {
                var payloadName = GetFileName(key);
                var payloadFile = Path.Combine(path, payloadName);
                using var fileStream = new FileStream(payloadFile, FileMode.Create, FileAccess.Write);
                EncryptedPayloadWriter.WriteEncryptedPayload(fileStream, payload);

                return Task.CompletedTask;
            }
            finally
            {
                semaphoreSlim.ExitWriteLock();
            }
        }

        public Task<EncryptedPayload?> GetEncryptedPayloadAsync(string key)
        {
            semaphoreSlim.EnterReadLock();
            try
            {
                EncryptedPayload? encryptedPayload;

                var payloadName = GetFileName(key);
                var payloadFile = Path.Combine(path, payloadName);
                if (!File.Exists(payloadFile))
                {
                    return Task.FromResult<EncryptedPayload?>(null);
                }
                using var fileStream = new FileStream(payloadFile, FileMode.Open, FileAccess.Read);
                encryptedPayload = EncryptedPayloadWriter.ReadEncryptedPayload(fileStream);

                return Task.FromResult<EncryptedPayload?>(encryptedPayload);
            }
            finally
            {
                semaphoreSlim.ExitReadLock();
            }
        }

        public Task ClearValueAsync(string key)
        {
            try
            {
                semaphoreSlim.EnterWriteLock();
                var payloadName = GetFileName(key);
                var payloadFile = Path.Combine(path, payloadName);
                if (File.Exists(payloadFile))
                {
                    File.Delete(payloadFile);
                }

                return Task.CompletedTask;
            }
            finally
            {
                semaphoreSlim.ExitWriteLock();
            }
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async IAsyncEnumerable<string> EnumerateKeysAsync()
        {
            var payloadPath = this.path;
            if (!Directory.Exists(payloadPath))
            {
                throw new DirectoryNotFoundException($"The directory {payloadPath} does not exist.");
            }

            var files = Directory.EnumerateFiles(payloadPath, "*.epl");
            foreach (var file in files)
            {
                var fileName = Path.GetFileNameWithoutExtension(file);
                if (fileName is not null)
                {
                    yield return fileName;
                }
            }
        }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

        public Task<bool> ContainsKeyAsync(string key)
        {
            var payloadName = GetFileName(key);
            var payloadFile = Path.Combine(path, payloadName);
            var keyExists = File.Exists(payloadFile);

            return Task.FromResult(keyExists);
        }

        public Task EvacuateCacheAsync(string? key = null)
        {
            // Nothing to do here
            return Task.CompletedTask;
        }
    }
}
