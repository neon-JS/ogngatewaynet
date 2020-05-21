using System;
using System.Collections.Generic;
using System.Linq;

namespace OgnGateway.extensions
{
    public static class StringExtension
    {
        public static void EnsureNotEmpty(this string subject)
        {
            if (string.IsNullOrEmpty(subject) || string.IsNullOrWhiteSpace(subject))
            {
                throw new NullReferenceException(nameof(subject) + " is null or empty!");
            }
        }
        
        public static IEnumerable<byte> ToBytes(this string s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));

            return s.ToList()
                .Select(c => (byte) c);
        }
    }
}