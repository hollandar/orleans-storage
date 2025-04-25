using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webefinity.Crypt.Json.Options
{
    public abstract class JsonCryptOptionsBase
    {
        // Key should be 16, 24 or 32 bytes bytes
        public string Key { get; set; } = string.Empty;
    }
}
