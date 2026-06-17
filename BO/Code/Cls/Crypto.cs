using System;
using System.Text;
using System.Security.Cryptography;
using System.Net.Http.Headers;
using System.Runtime.Intrinsics.X86;

namespace BO.Code.Cls
{
    public class Crypto
    {
        private string _key { get; set; } = "Load1None2Hell3";
      
        private static byte[] MD5Hash(string value)
        {
            
            using (MD5 md5 = MD5.Create())
            {
                return md5.ComputeHash(UTF8Encoding.UTF8.GetBytes(value));
            }
        }

        public string Encrypt(string strExpression,string key=null)
        {
            if (key == null) key = _key;

            using (TripleDES des = TripleDES.Create())
            {
                des.Key = MD5Hash(key);
                des.Mode = CipherMode.ECB;
                byte[] buffer = UTF8Encoding.UTF8.GetBytes(strExpression);
                return Convert.ToBase64String(des.CreateEncryptor().TransformFinalBlock(buffer, 0, buffer.Length));
            }

            
        }

        public string Decrypt(string strExpression,string key = null)
        {
            if (key == null) key = _key;
            using (TripleDES des = TripleDES.Create())
            {
                try
                {
                    des.Key = MD5Hash(key);
                    des.Mode = CipherMode.ECB;
                    byte[] buffer = Convert.FromBase64String(strExpression);
                    return UTF8Encoding.UTF8.GetString(des.CreateDecryptor().TransformFinalBlock(buffer, 0, buffer.Length));
                }
                catch
                {
                    return null;
                }
                
            }
        
        }


        public string EasyEncrypt(string strExpression)
        {
            string s = Convert.ToBase64String(Encoding.UTF8.GetBytes(strExpression));

            return s.Substring(0, 4) + new Random().Next(0, 9).ToString().ToString() + s.Substring(4, s.Length - 4);
        }
        public string EasyDecrypt(string strExpression)
        {
            string s = strExpression;
            s = s.Substring(0, 4) + s.Substring(5, s.Length - 5);
            var base64EncodedBytes = System.Convert.FromBase64String(s);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}
