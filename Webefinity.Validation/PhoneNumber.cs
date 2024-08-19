using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webefinity.Validation
{
    public static class PhoneNumber
    {
        public static bool IsValidPhoneNumber(this string input, bool isRequired = false) => (isRequired == false && string.IsNullOrEmpty(input)) || input.All(r => char.IsDigit(r) || r == ' ');
    }
}
