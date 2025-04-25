using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webefinity.Crypt.Json
{
    public class HashException : Exception
    {
        public HashException()
        {
        }

        public HashException(string? message) : base(message)
        {
        }
    }
}
