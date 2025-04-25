using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webefinity.Crypt.Json
{
    public class FileLengthException : Exception
    {
        public FileLengthException()
        {
        }

        public FileLengthException(string? message) : base(message)
        {
        }
    }
}
