using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Barcodes;

public class Gs1Barcode
{
    private static readonly HashSet<int> validLengths = new HashSet<int> { 8, 12, 13, 14 };

    public static HashSet<int> ValidLengths => validLengths;

    public static bool IsValidLength(int length) => validLengths.Contains(length);

    public static ValueResult<bool> CheckBarcode(string barcode)
    {
        if (!IsValidLength(barcode.Length))
        {
            return ValueResult<bool>.Fail("Invalid barcode length, valid lengths are 8, 12, 13, 14.  Remember that the last digit is always a check digit.");
        }

        var codePart = barcode.Substring(0, barcode.Length - 1);
        var checkDigit = barcode[barcode.Length - 1];
        return ValueResult<bool>.Ok(CalculateCheckDigit(codePart) == checkDigit);
    }

    public static char CalculateCheckDigit(string barcode)
    {
        var sum = 0;
        var odd = true;
        foreach (var c in barcode)
        {
            if (c < '0' || c > '9')
            {
                throw new ArgumentException("Barcode must contain only digits");
            }
            var digit = c - '0';
            if (odd)
            {
                sum += digit;
            }
            else
            {
                sum += digit * 3;
            }
            odd = !odd;
        }
        var checkDigit = (10 - (sum % 10)) % 10;
        return (char)('0' + checkDigit);
    }
}
