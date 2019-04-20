using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace IDO_API.DataBase.Hashing
{
    public class SHA
    {
        const string salt = "QYUEJHQCBABK2121NA241SMFhaHSAb";
        public static string GenerateSaltedHashBase64(string text)
        {
            HashAlgorithm algorithm = new SHA256Managed();

            var salt  = Encoding.UTF8.GetBytes(SHA.salt);
            var textBytes  = Encoding.UTF8.GetBytes(text);

            var saltWithTextBytes = new byte[textBytes.Length + salt.Length];

            textBytes.CopyTo(saltWithTextBytes, 0);
            salt.CopyTo(saltWithTextBytes, textBytes.Length);

            var hash = algorithm.ComputeHash(saltWithTextBytes);
            return Convert.ToBase64String(hash);
        }

    }
}
