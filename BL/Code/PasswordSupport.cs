using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;

namespace BL.Code
{
    public class PasswordSupport
    {
        //Password Checker:
        const string _LOWER_CASE = "abcdefghijklmnopqursuvwxyz";
        const string _UPPER_CAES = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string _NUMBERS = "123456789";
        const string _SPECIALS = @"!@.$_!&*()#.";
        private bool RequireDigit { get; set; }
        private bool RequireLowercase { get; set; }
        private bool RequireUppercase { get; set; }
        private bool RequireNonAlphanumeric { get; set; }
        private int MinLength { get; set; }
        private int MaxLength { get; set; }


        //Password Hasher:
        public byte Version => 1;
        public int Pbkdf2IterCount { get; } = 50000;
        public int Pbkdf2SubkeyLength { get; } = 256 / 8; // 256 bits
        public int SaltSize { get; } = 256 / 8; // 256 bits
        //public int SaltSize { get; } = 128 / 8; // 128 bits
        public HashAlgorithmName HashAlgorithmName { get; } = HashAlgorithmName.SHA256;

        public PasswordSupport()
        {
            var strFolder = System.IO.Directory.GetCurrentDirectory();
            var config = new ConfigurationBuilder().AddJsonFile(strFolder + "\\appsettings.json", true).Build();
            this.MinLength = Convert.ToInt32(config.GetSection("PasswordChecker")["MinLength"]);
            this.MaxLength = Convert.ToInt32(config.GetSection("PasswordChecker")["MaxLength"]);
            this.RequireDigit = Convert.ToBoolean(config.GetSection("PasswordChecker")["RequireDigit"]);
            this.RequireLowercase = Convert.ToBoolean(config.GetSection("PasswordChecker")["RequireLowercase"]);
            this.RequireUppercase = Convert.ToBoolean(config.GetSection("PasswordChecker")["RequireUppercase"]);
            this.RequireNonAlphanumeric = Convert.ToBoolean(config.GetSection("PasswordChecker")["RequireNonAlphanumeric"]);
        }

        public BO.Result CheckPassword(string pass)
        {
            //min 6 chars, max 12 chars
            if (pass.Length < this.MinLength || pass.Length > this.MaxLength)
                return new BO.Result(true, string.Format("Minimální délka hesla je: {0} znaků, maximální délka: {1} znaků.", this.MinLength, this.MaxLength));

            //No white space
            if (pass.Contains(" "))
                return new BO.Result(true, "Heslo nesmí obsahovat mezeru.");

            //At least 1 upper case letter
            if (this.RequireUppercase && !pass.Any(char.IsUpper))
                return new BO.Result(true, "Heslo musí obsahovat minimálně jedno velké písmeno.");

            //At least 1 lower case letter
            if (this.RequireLowercase && !pass.Any(char.IsLower))
                return new BO.Result(true, "Heslo musí obsahovat minimálně jedno malé písmeno.");

            if (this.RequireDigit)
            {
                //At least 1 numer                
                char[] numbersArray = _NUMBERS.ToCharArray();
                bool b = false;
                foreach (char c in numbersArray)
                {
                    if (pass.Contains(c))
                        b = true;
                }
                if (!b)
                {
                    return new BO.Result(true, "Heslo musí obsahovat minimálně jedno číslo.");
                }
            }



            if (this.RequireNonAlphanumeric)
            {
                //At least 1 special char                
                char[] specialCharactersArray = _SPECIALS.ToCharArray();
                bool b = false;
                foreach (char c in specialCharactersArray)
                {
                    if (pass.Contains(c))
                        b = true;
                }
                if (!b)
                {
                    return new BO.Result(true, "Heslo musí obsahovat minimálně jeden non-alfanumerický znak.");
                }
            }


            return new BO.Result(false, "ok");
        }



        public string GetRandomPassword()
        {

            System.Random rnd = new Random();
            string ret = "";

            // Build up the character set to choose from
            if (this.RequireLowercase) ret += _LOWER_CASE[rnd.Next(_LOWER_CASE.Length - 1)];
            if (this.RequireUppercase) ret += _UPPER_CAES[rnd.Next(_UPPER_CAES.Length - 1)];
            if (this.RequireDigit) ret += _NUMBERS[rnd.Next(_NUMBERS.Length - 1)];
            if (this.RequireNonAlphanumeric) ret += _SPECIALS[rnd.Next(_SPECIALS.Length - 1)];
            ret += BO.Code.Bas.GetGuid();

            return ret.Substring(0, this.MinLength);


        }


        //Password Hasher:
        public string HashPassword(string password)
        {
            if (password == null)
                throw new ArgumentNullException(nameof(password));

            byte[] salt;
            byte[] bytes;
            using (var rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, SaltSize, Pbkdf2IterCount, HashAlgorithmName))
            {
                salt = rfc2898DeriveBytes.Salt;
                bytes = rfc2898DeriveBytes.GetBytes(Pbkdf2SubkeyLength);
            }

            var inArray = new byte[1 + SaltSize + Pbkdf2SubkeyLength];
            inArray[0] = Version;
            Buffer.BlockCopy(salt, 0, inArray, 1, SaltSize);
            Buffer.BlockCopy(bytes, 0, inArray, 1 + SaltSize, Pbkdf2SubkeyLength);

            return Convert.ToBase64String(inArray);
        }

        public BO.Result VerifyUserPassword(string strPwd, string strLogin, BO.RunningUser user)
        {
            var hasher = new BO.Code.Cls.PasswordHasher();
            var overeni = VerifyHashedPassword(user.j02PasswordHash, getUserSul(strLogin, strPwd, user.pid));
            if (overeni==BO.ResultEnum.Failed)
            {

                return new BO.Result(true, "Ověření uživatele se nezdařilo - pravděpodobně chybné heslo nebo jméno!");
            }
            else
            {
                return new BO.Result(false);
            }
        }

        public string GetPasswordHash(string strPwd, string strLogin,int intPID)
        {
            
            return HashPassword(getUserSul(strLogin, strPwd, intPID));
        }

        private string getUserSul(string strLogin, string strPwd, int intPid)
        {
            return strLogin.ToUpper() + "+kurkuma+" + strPwd + "+" + intPid.ToString();
        }

        private BO.ResultEnum VerifyHashedPassword(string hashedPassword, string password)
        {
            if (password == null)
                throw new ArgumentNullException(nameof(password));

            if (hashedPassword == null)
                return BO.ResultEnum.Failed;

            byte[] numArray = Convert.FromBase64String(hashedPassword);
            if (numArray.Length < 1)
                return BO.ResultEnum.Failed;

            byte version = numArray[0];
            if (version > Version)
                return BO.ResultEnum.Failed;

            byte[] salt = new byte[SaltSize];
            Buffer.BlockCopy(numArray, 1, salt, 0, SaltSize);
            byte[] a = new byte[Pbkdf2SubkeyLength];
            Buffer.BlockCopy(numArray, 1 + SaltSize, a, 0, Pbkdf2SubkeyLength);
            byte[] bytes;
            using (var rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, salt, Pbkdf2IterCount, HashAlgorithmName))
            {
                bytes = rfc2898DeriveBytes.GetBytes(Pbkdf2SubkeyLength);
            }

            if (FixedTimeEquals(a, bytes))
                return BO.ResultEnum.Success;



            return BO.ResultEnum.Failed;
        }


        
        public static bool FixedTimeEquals(byte[] left, byte[] right)
        {
            // NoOptimization because we want this method to be exactly as non-short-circuiting as written.
            // NoInlining because the NoOptimization would get lost if the method got inlined.
            if (left.Length != right.Length)
            {
                return false;
            }

            int length = left.Length;
            int accum = 0;

            for (int i = 0; i < length; i++)
            {
                accum |= left[i] - right[i];
            }

            return accum == 0;
        }
    }
}

