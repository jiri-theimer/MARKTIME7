
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace BO.Code
{
    public static class Bas
    {
        public static string GB(bool b)
        {
            if (b)
            {
                return "1";
            }
            else
            {
                return "0";
            }
        }
        public static bool BG(string b)
        {
            if (b == null) return false;

            if (b=="1" || b.ToLower()=="true")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static string GS(string s)
        {
            if(string.IsNullOrEmpty(s) == true)
            {
                return "NULL";
            }
            if (s.Contains("'"))
            {
                s = s.Replace("'", "''");
            }
            //s = s.Replace("'", "");
            return $"'{s}'";
            //return "'" + s + "'";
        }
        public static string GSS(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return "";
            }
            s = s.Replace("'", "").Replace("--", "##");
            return s;
        }
        public static string GD(DateTime? d, DateTime? valIfNull=null)
        {
            if (d == null)
            {
                if (valIfNull == null)
                {
                    return "NULL";
                }
                else
                {
                    d = valIfNull;
                }
                
            }
            return $"CONVERT(datetime,'{ObjectDate2String(d, "dd.MM.yyyy HH:mm:ss")}',104)";
        }
        public static int YearMonthInteger(DateTime d)
        {
            return Convert.ToInt32(d.ToString("yyyyMM"));
        }
        public static string YearMonthString(DateTime d)
        {
            return d.ToString("yyyy-MM");
        }

        public static string GN(double n)
        {
            return n.ToString().Replace(",", ".");
        }
        public static string IIFS(bool b,string value_true,string value_false)
        {
            if (b)
            {
                return value_true;
            }
            else
            {
                return value_false;
            }
        }
        public static string OM2(string s,int maxlen)
        {
            if (s == null) return null;
            if (s.Length > maxlen)
            {
                return $"{s.Substring(0, maxlen - 1)}...";                
            }
            else
            {
                return s;
            }
        }
        public static string LeftString(string s, int maxlen)
        {
            if (s == null) return null;
            if (s.Length > maxlen)
            {
                return s.Substring(0, maxlen);
            }
            else
            {
                return s;
            }
        }
        public static int InInt(string s)
        {
            if (int.TryParse(s, out int x))
            {
                return x;
            }
            else
            {
                return 0;
            }
        }
        public static Double InDouble(string s)
        {
            if (string.IsNullOrEmpty(s)) return 0.00;            
            if (double.TryParse(s, out Double x))
            {
                return x;
            }
            else
            {
                if (s.Contains("."))
                {
                    s = s.Replace(".", ",");
                    if (double.TryParse(s, out Double y))
                    {
                        return y;
                    }
                }
                return 0.00;
            }
        }

        public static List<string> ConvertString2List(string s, string strDelimiter = ",")
        {
            var lis = new List<string>();

            if (s == null)
                return lis;
            
            lis.AddRange(s.Split(strDelimiter));

            return lis;
        }
        public static List<int> ConvertString2ListInt(string s, string strDelimiter = ",")
        {
            var lis = new List<int>();

            if (String.IsNullOrEmpty(s))
                return lis;

            foreach(var ss in s.Split(strDelimiter))
            {
                lis.Add(InInt(ss));
            }
            
            return lis;
        }
        public static string RemoveValueFromDelimitedString(string s,string sremove, string strDelimiter = ",")
        {
            List<string> lis = ConvertString2List(s, strDelimiter);
            if (lis.Contains(sremove))
            {
                lis.Remove(sremove);
            }
            
            return String.Join(strDelimiter, lis);
        }

        public static int? TestIntAsDbKey(int intPID)
        {
            if (intPID == 0)
            {
                return null;
            }
            else
            {
                return intPID;
            }
        }
        public static double? TestDouleAsDbKey(double dbl)
        {
            if (dbl == 0)
            {
                return null;
            }
            else
            {
                return dbl;
            }
        }
        public static decimal? TestDecimalAsDbKey(decimal dcl)
        {
            if (dcl == 0)
            {
                return null;
            }
            else
            {
                return dcl;
            }
        }
        
        public static DateTime String2Date(string d)
        {            

            if (d.IndexOf(". ") > 0)
            {
                d = d.Replace(". ", ".");
                
            }
            string[] arr = d.Split(".");
            if (arr.Length < 3) return (DateTime.Today);
            
            if (arr[2].Contains(":"))
            {                //obsahuje čas: dd.MM.yyyy HH:mm
                string[] hhmm= arr[2].Substring(5,arr[2].Length-5).Split(":");
                arr[2] = arr[2].Substring(0, 4);
                int h = int.Parse(hhmm[0]);
                int m = int.Parse(hhmm[1]);
                return (new DateTime(int.Parse(arr[2]), int.Parse(arr[1]), int.Parse(arr[0]),h,m,0));
            }
            else
            {
               
                return (new DateTime(int.Parse(arr[2].Substring(0, 4)), int.Parse(arr[1]), int.Parse(arr[0])));
            }
            
        }
        public static string DayOfWeekString(DateTime d)
        {
            switch ((int)d.DayOfWeek)
            {
                case 0:
                    return "Neděle";
                case 1:
                    return "Pondělí";
                case 2:
                    return "Úterý";
                case 3:
                    return "Středa";                
                case 4:
                    return "Čtvrtek";
                case 5:
                    return "Pátek";
                case 6:
                    return "Sobota";                
                default:
                    return "??";
            }
        }

        public static DateTime ConvertDateTo235959(DateTime d)
        {
            
            return new DateTime(d.Year, d.Month, d.Day).AddDays(1).AddSeconds(-1);
        }
        public static string ObjectDate2String(object d,string format="dd.MM.yyyy ddd")
        {
            if (d == System.DBNull.Value || d==null) return "";
           
            return Convert.ToDateTime(d).ToString(format);
        }
        public static string ObjectDateTime2String(object d, string format = "dd.MM.yyyy HH:mm")
        {            
            return ObjectDate2String(d, format);
        }
        public static string Number2String(double n)
        {
            return string.Format("{0:#,0.00}",n);
        }
        public static string Integer2String(int n)
        {
            return string.Format("{0:#,0}", n);
        }

        public static string Num2StringNull(double n)
        {
            if (n == 0) return null;
            return string.Format("{0:#,0.00}", n);
        }

        public static string GetGuid()
        {

            return System.Guid.NewGuid().ToString("N");
        }
       


        public static string FormatFileSize(int byteCount)
        {
            string size = "0 Bytes";
            if (byteCount >= 1073741824)
                size = String.Format("{0:##.##}", byteCount / 1073741824) + " GB";
            else if (byteCount >= 1048576)
                size = String.Format("{0:##.##}", byteCount / 1048576) + " MB";
            else if (byteCount >= 1024)
                size = String.Format("{0:##.##}", byteCount / 1024) + " KB";
            else if (byteCount > 0 && byteCount < 1024)
                size = byteCount.ToString() + " Bytes";

            return size;
        }


        public static string ParseCellValueFromDb(System.Data.DataRow dbRow, BO.TheGridColumn c,string defval=null,string defhoursformat="N")
        {
            if (dbRow[c.UniqueName] == System.DBNull.Value)
            {
                return defval;
            }
            
            switch (c.FieldType)
            {
                case "bool":
                    if (Convert.ToBoolean(dbRow[c.UniqueName]) == true)
                    {
                        return "&#10004;";
                    }
                    else
                    {
                        return "";
                    }
                case "num0":
                    return string.Format("{0:#,0}", dbRow[c.UniqueName]);

                case "num":
                    if (c.IsHours && defhoursformat=="T")
                    {
                        if (dbRow[c.UniqueName] == System.DBNull.Value)
                        {
                            return null;
                        }
                        else
                        {
                            return BO.Code.Time.ShowAssHHMM(Convert.ToDouble(dbRow[c.UniqueName]));
                        }
                    }
                    return string.Format("{0:#,0.00}", dbRow[c.UniqueName]);                    
                    
                case "num3":
                    return string.Format("{0:#,0.000}", dbRow[c.UniqueName]);
                case "num4":
                    return string.Format("{0:#,0.0000}", dbRow[c.UniqueName]);
                case "num5":
                    return string.Format("{0:#,0.00000}", dbRow[c.UniqueName]);
                case "num1":
                    return string.Format("{0:#,0.0}", dbRow[c.UniqueName]);

                
                case "date":
                    return Convert.ToDateTime(dbRow[c.UniqueName]).ToString("dd.MM.yyyy");


                case "datetime":

                    return Convert.ToDateTime(dbRow[c.UniqueName]).ToString("dd.MM.yyyy HH:mm");
                case "datetimesec":

                    return Convert.ToDateTime(dbRow[c.UniqueName]).ToString("dd.MM.yyyy HH:mm:ss");
                case "time":
                    return Convert.ToDateTime(dbRow[c.UniqueName]).ToString("HH:mm");
                case "filesize":
                    return BO.Code.Bas.FormatFileSize(Convert.ToInt32(dbRow[c.UniqueName]));


                default:
                    return dbRow[c.UniqueName].ToString();
            }


        }

        public static string RightString(string input, int num)
        {
            if (num > input.Length)
            {
                num = input.Length;
            }
            return input.Substring(input.Length - num);
        }

        public static string Uvozovky2Apostrofy(string s)
        {
            if (string.IsNullOrEmpty(s) == true)
            {
                return s;
            }
            if (s.Contains(Char.ConvertFromUtf32(34)))
            {
                return s.Replace(Char.ConvertFromUtf32(34), "'");
            }
            else
            {
                return s;
            }
        }

        public static bool IsValidEmail(string email)
        {
            try
            {
                email = email.Trim();
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public static string EncryptedString()
        {
            return "********";
        }

        public static string Text2Html(string s)
        {
            if (s == null) return s;
            if (s.Contains("\r\n"))
            {
                return s.Replace("\r\n", "<br>");
            }
            else
            {
                if (s.Contains("\r"))
                {
                    return s.Replace("\r", "<br>");
                }
            }

            return s;
        }


        public static string OcistitSQL(string strSQL)
        {
            if (strSQL == null) return null;

            strSQL = strSQL.Replace("/*", "");
            strSQL = strSQL.Replace("*/", "");
            strSQL = strSQL.Replace("--", "");
            strSQL = strSQL.Replace("drop ", "", StringComparison.OrdinalIgnoreCase);
            strSQL = strSQL.Replace("truncate ", "", StringComparison.OrdinalIgnoreCase);
            strSQL = strSQL.Replace("delete ", "", StringComparison.OrdinalIgnoreCase);

            return strSQL;

        }

        public static string RemoveDiacritics(string input)
        {
            if (input == null)
                return null;

            // Normalizace na FormD (znaky + diakritické značky separátně)
            var normalized = input.Normalize(NormalizationForm.FormD);

            // Vyřazení diakritických značek
            var filtered = normalized
                .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                .ToArray();

            // Vrácení zpět do FormC
            return new string(filtered).Normalize(NormalizationForm.FormC);
        }


        public static string ParseDbNameFromCloudLogin(string strLogin)
        {
            int pos = strLogin.IndexOf("@");
            if (pos == -1)
                return "";

            int tecka = strLogin.IndexOf(".", pos);
            if (tecka > -1)
                tecka = strLogin.Length - tecka;
            else
                tecka = 0;

            return strLogin.Substring(pos + 1, strLogin.Length - pos - 1 - tecka);
        }

        


        public static string OcistiSQL_WildCards(string strFilterValue)
        {
            //podtržítko je wildcard znak - podobně jako %!!!!
            if (strFilterValue == null) return null;
            if (strFilterValue.Contains("_"))
            {
                strFilterValue = strFilterValue.Replace("_", "[\\_]");    //opravdu hledat podle _ (podtržítko)
            }
            return strFilterValue;
        }

        public static bool TestPermissionInRoleValue(string rolevalue,BO.PermValEnum permpos)
        {
            if (rolevalue.Substring((int)permpos - 1, 1) == "1") return true;


            return false;
        }

        public static bool bit_compare_or(int intBitTotalValue, int intOneValue)
        {
            if (intBitTotalValue == 0 || intOneValue==0)
                return false;

            if ((intBitTotalValue & intOneValue) == intOneValue)
                return true;


            return false;
        }

        public static string Html2Text(string html)
        {
            if (string.IsNullOrWhiteSpace(html)) return null;

            // could be stored in static variable
            var regex = new Regex("<[^>]+>|\\s{2}", RegexOptions.IgnoreCase);
            return System.Web.HttpUtility.HtmlDecode(regex.Replace(html, ""));
        }

        public static DateTime get_first_prev_monday(DateTime d)
        {
            for (int i = 0; i <= 7; i++)
            {
                if (d.DayOfWeek == DayOfWeek.Monday)
                {
                    return d;
                }
                d = d.AddDays(-1);
            }
            return d;
        }

        public static bool Diff2String(string s1, string s2)
        {
            if (string.IsNullOrEmpty(s1) && !string.IsNullOrEmpty(s2)) return true;
            if (!string.IsNullOrEmpty(s1) && string.IsNullOrEmpty(s2)) return true;
            
            if (!string.IsNullOrEmpty(s1))
            {
                if (s1.Trim().Replace(" ", "").ToLower() != s2.Trim().ToLower().Replace(" ", "")) return true;
            }
            
            return false;
        }

    }

}




    

