using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PasswordCrackerClient
{
    public static class StringExtensions
    {
        public static string Reverse(this string str)
        {
            if (str == null)
            {
                throw new ArgumentNullException("str");
            }
            if (str.Length == 0)
            {
                return str;
            }
            StringBuilder reverseString = new StringBuilder();
            for (int i = 0; i < str.Length; i++)
            {
                reverseString.Append(str.ElementAt(str.Length - 1 - i));
            }
            return reverseString.ToString();
        }
        public static string CapitalizeLettersFromStart(this string str, int count)
        {
            StringBuilder stringBuilder = new StringBuilder(str);
            for (int i = 0; i < str.Length && i < count; i++)
            {
                stringBuilder.Insert(i, Char.ToUpper(str[i]));
            }
            return stringBuilder.ToString();
        }
    }
}
