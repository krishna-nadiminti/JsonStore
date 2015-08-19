using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Money.Common.Extensions
{
    public static class StringExtensions
    {
        public static bool IsEmpty(this string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return true;

            s = s.Trim();

            if (string.IsNullOrWhiteSpace(s))
                return true;

            return false;
        }
    }
}
