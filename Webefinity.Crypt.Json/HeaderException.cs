using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webefinity.Crypt.Json
{
    public class HeaderException : Exception
    {
        public HeaderException()
        {
        }

        public HeaderException(string? message) : base(message)
        {
        }
    }
}
