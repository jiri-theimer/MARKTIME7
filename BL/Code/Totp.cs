using System.Security.Cryptography;
using System.Text;

namespace BL.Code
{
    /// <summary>
    /// Jednorázové kódy ověřovací aplikace (TOTP, RFC 6238): HMAC-SHA1, krok 30 sekund, 6 číslic.
    /// Stejný standard používá Microsoft Authenticator, Google Authenticator, Aegis, 2FAS i správci hesel –
    /// aplikace si tajný klíč načte z QR kódu (adresa otpauth://) a dál počítá kódy sama, bez spojení s MARKTIME.
    /// </summary>
    public static class Totp
    {
        private const string Base32Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        private const int StepSeconds = 30;
        private const int Digits = 6;

        /// <summary>Nový tajný klíč (160 bitů) v zápisu Base32 – ukládá se do j02User.j02TotpSecret.</summary>
        public static string GenerateSecret()
        {
            var bytes = new byte[20];
            RandomNumberGenerator.Fill(bytes);
            return ToBase32(bytes);
        }

        /// <summary>Náhodný číselný kód (např. pro ověření e-mailem).</summary>
        public static string GenerateNumericCode(int digits)
        {
            int max = (int)Math.Pow(10, digits);
            return RandomNumberGenerator.GetInt32(0, max).ToString().PadLeft(digits, '0');
        }

        /// <summary>Adresa pro QR kód. issuer = název zobrazený v aplikaci (MARKTIME), account = login uživatele.</summary>
        public static string BuildUri(string issuer, string account, string secret)
        {
            string strIssuer = Uri.EscapeDataString(issuer ?? "MARKTIME");
            string strAccount = Uri.EscapeDataString(account ?? "");
            return $"otpauth://totp/{strIssuer}:{strAccount}?secret={secret}&issuer={strIssuer}&algorithm=SHA1&digits={Digits}&period={StepSeconds}";
        }

        /// <summary>Tajný klíč po čtveřicích – pro ruční opsání do aplikace, když nejde načíst QR kód.</summary>
        public static string FormatSecret(string secret)
        {
            if (string.IsNullOrEmpty(secret)) return "";
            var sb = new StringBuilder();
            for (int i = 0; i < secret.Length; i++)
            {
                if (i > 0 && i % 4 == 0) sb.Append(' ');
                sb.Append(secret[i]);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Ověří kód z aplikace. Kvůli nepřesným hodinám telefonu platí i kód z předchozího a následujícího kroku (± 30 sekund).
        /// </summary>
        public static bool Verify(string secret, string code)
        {
            if (string.IsNullOrEmpty(secret) || string.IsNullOrEmpty(code)) return false;
            code = new string(code.Where(char.IsDigit).ToArray());     //uživatel může kód opsat s mezerou: "123 456"
            if (code.Length != Digits) return false;

            byte[] key;
            try
            {
                key = FromBase32(secret);
            }
            catch
            {
                return false;
            }
            if (key.Length == 0) return false;

            long counter = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / StepSeconds;
            bool bolOK = false;
            for (long i = -1; i <= 1; i++)
            {
                //porovnání ve stálém čase a bez předčasného ukončení cyklu
                if (CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(Compute(key, counter + i)), Encoding.ASCII.GetBytes(code)))
                {
                    bolOK = true;
                }
            }
            return bolOK;
        }

        private static string Compute(byte[] key, long counter)
        {
            var msg = BitConverter.GetBytes(counter);
            if (BitConverter.IsLittleEndian) Array.Reverse(msg);    //čítač jako 8 bajtů big-endian

            byte[] hash;
            using (var hmac = new HMACSHA1(key))
            {
                hash = hmac.ComputeHash(msg);
            }
            int offset = hash[hash.Length - 1] & 0x0F;
            int binary = ((hash[offset] & 0x7F) << 24) | (hash[offset + 1] << 16) | (hash[offset + 2] << 8) | hash[offset + 3];
            return (binary % 1000000).ToString().PadLeft(Digits, '0');
        }

        private static string ToBase32(byte[] data)
        {
            var sb = new StringBuilder();
            int buffer = 0; int bits = 0;
            foreach (byte b in data)
            {
                buffer = (buffer << 8) | b;
                bits += 8;
                while (bits >= 5)
                {
                    sb.Append(Base32Chars[(buffer >> (bits - 5)) & 31]);
                    bits -= 5;
                }
            }
            if (bits > 0)
            {
                sb.Append(Base32Chars[(buffer << (5 - bits)) & 31]);
            }
            return sb.ToString();
        }

        private static byte[] FromBase32(string s)
        {
            s = (s ?? "").Trim().Replace(" ", "").TrimEnd('=').ToUpperInvariant();
            var ret = new List<byte>();
            int buffer = 0; int bits = 0;
            foreach (char ch in s)
            {
                int val = Base32Chars.IndexOf(ch);
                if (val < 0) throw new FormatException("Base32");
                buffer = (buffer << 5) | val;
                bits += 5;
                if (bits >= 8)
                {
                    ret.Add((byte)((buffer >> (bits - 8)) & 0xFF));
                    bits -= 8;
                }
            }
            return ret.ToArray();
        }
    }
}
