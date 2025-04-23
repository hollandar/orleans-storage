using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webefinity.Auth.API.Options
{
    public class ApiKeyOptions
    {
        public string Endpoint { get; set; } = string.Empty;
        public string[] ApiKeys { get; set; } = Array.Empty<string>();
    }
}
