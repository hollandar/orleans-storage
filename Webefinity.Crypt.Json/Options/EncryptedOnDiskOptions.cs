using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webefinity.Crypt.Json.Options;

public class EncryptedOnDiskOptions : JsonCryptOptionsBase
{
    public string Path { get; set; } = string.Empty;
}
