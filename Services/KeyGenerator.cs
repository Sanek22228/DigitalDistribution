using System.Text;
using System.Security.Cryptography;

namespace DigitalDistribution.Services
{
    public static class KeyGenerator
    {
        private const string Chars = "23456789QWERTYUPASDFGHJKLZXCVBNM";
        public static string GenerateKey(int groupCount = 5, int groupLength = 4)
        {
            // instead of Random I use RandomNumberGenerator it's unpredictable and works better            
            byte[] arr = new byte[groupCount * groupLength];
            using(RandomNumberGenerator rn = RandomNumberGenerator.Create())
                rn.GetBytes(arr, 0, arr.Length);

            // instead of string += I use StringBuilder it doesn't create a new string and allocate memory (strings are immutable) 
            var resultString = new StringBuilder();
            
            for (int i  = 0; i < arr.Length; i++)
            {
                if (i > 0 && i % groupLength == 0)
                    resultString.Append('-');

                resultString.Append(Chars[arr[i] % Chars.Length]);
            }

            return resultString.ToString();
        }
    }
}
