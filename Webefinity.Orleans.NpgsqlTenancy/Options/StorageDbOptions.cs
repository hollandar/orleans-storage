using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orleans.NpgsqlTenancy.Options
{
    public class TenancyStorageOptions
    {
        public Dictionary<string, StorageDbOptions> Storages { get; set; } = new();
    }

    public class StorageDbOptions
    {
        public string Host { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Database { get; set; } = null;
        public int Port { get; set; } = 5432;
    }
}
