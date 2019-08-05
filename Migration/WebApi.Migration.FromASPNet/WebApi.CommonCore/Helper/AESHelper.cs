using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace WebApi.CommonCore.Helper
{
    public class AESHelper
    {
        [Obsolete("Remove after key vault migration complete for all project.")]
        public static string AESKey { get; set; }
        [Obsolete("Remove after key vault migration complete for all project.")]
        public static string AESKeyACH { get; set; }  //Key for encypt ach content file

        public static string DescryptAES(string encryptStr, string key)
        {
            var encryptedBytes = Convert.FromBase64String(encryptStr);
            var result = Encoding.UTF8.GetString(Decrypt(encryptedBytes, GetRijndaelManaged(key)));
            return result;
        }

        public static string EncryptAES(string content, string key)
        {
            var plainBytes = Encoding.UTF8.GetBytes(content);
            var result = Convert.ToBase64String(Encrypt(plainBytes, GetRijndaelManaged(key)));
            return result;
        }

        public static byte[] EncryptAES(byte[] inputContent, string key)
        {
            return Encrypt(inputContent, GetRijndaelManaged(key));
        }

        public static byte[] DescryptAES(byte[] inputContent, string key)
        {
            return Decrypt(inputContent, GetRijndaelManaged(key));
        }


        private static RijndaelManaged GetRijndaelManaged(String secretKey)
        {
            var keyBytes = new byte[16];
            var secretKeyBytes = Encoding.UTF8.GetBytes(secretKey);
            Array.Copy(secretKeyBytes, keyBytes, Math.Min(keyBytes.Length, secretKeyBytes.Length));
            return new RijndaelManaged
            {
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7,
                KeySize = 128,
                BlockSize = 128,
                Key = keyBytes,
                IV = keyBytes
            };
        }

        private static byte[] Encrypt(byte[] plainBytes, RijndaelManaged rijndaelManaged)
        {
            return rijndaelManaged.CreateEncryptor()
                .TransformFinalBlock(plainBytes, 0, plainBytes.Length);
        }

        private static byte[] Decrypt(byte[] encryptedData, RijndaelManaged rijndaelManaged)
        {
            return rijndaelManaged.CreateDecryptor()
                .TransformFinalBlock(encryptedData, 0, encryptedData.Length);
        }
    }
}
