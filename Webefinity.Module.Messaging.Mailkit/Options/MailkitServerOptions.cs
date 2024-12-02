using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webefinity.Module.Messaging.Options
{
    public class MailkitServerOptions
    {
        public string? Host { get; set; } = null;
        public int Port { get; set; } = 432;
        public bool RequiresAuthentication { get; set; } = true;
        public string? Username { get; set; } = null;
        public string? Password { get; set; } = null;
        public bool UseSsl { get; set; } = true;
    }
}
