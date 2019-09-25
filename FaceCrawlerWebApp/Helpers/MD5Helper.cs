using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Helpers
{
    public static class MD5Helper
    {
        public static string Encrypt(string souce, bool lowerCase = true)
        {
            if (string.IsNullOrWhiteSpace(souce))
            {
                return string.Empty;
            }
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] md5Byte = md5.ComputeHash(Encoding.UTF8.GetBytes(souce));
            StringBuilder builder = new StringBuilder();
            foreach (byte item in md5Byte)
            {
                builder.Append(Math.Abs(item).ToString("x").PadLeft(2, '0'));
            }
            if (!lowerCase)
            {
                return builder.ToString().ToUpper();
            }
            return builder.ToString().ToLower();
        }
    }
}
