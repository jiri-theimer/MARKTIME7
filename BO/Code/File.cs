using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using System.Xml.Serialization;
using Microsoft.VisualBasic;

namespace BO.Code
{
    public class File
    {

        public static byte[] LoadFilefAsBytes(string strPath)
        {
            if (string.IsNullOrWhiteSpace(strPath))
                throw new ArgumentException("Cesta k PDF je prázdná.");

            if (!System.IO.File.Exists(strPath))
                throw new FileNotFoundException($"Soubor nebyl nalezen: {strPath}");

            return System.IO.File.ReadAllBytes(strPath);
        }
        public static string GetFileContent(string fullpath)
        {
            if (System.IO.File.Exists(fullpath))
            {
                //return System.IO.File.ReadAllText(fullpath, Encoding.UTF8);
                return System.IO.File.ReadAllText(fullpath);
            }
            else
            {
                return null;
            }
        }

        public static string GetLinesContent(string fullpath, int startrow, int endrow)
        {
            if (!System.IO.File.Exists(fullpath))
            {
                return null;
            }

            var sr = new StreamReader(fullpath, Encoding.UTF8);
            var sb = new StringBuilder();
            int x = 1;

            while (sr.Peek() >= 0)
            {
                string line = sr.ReadLine();
                if (x >= startrow && x <= endrow)
                {
                    if (endrow == startrow)
                    {
                        sb.Append(line);
                    }
                    else
                    {
                        sb.AppendLine(line);
                    }
                }
                x += 1;
            }
            sr.Close();


            return sb.ToString();

        }

        public static string PrepareFileName(string strFileName, bool bolZnicitTecky)
        {
            string s = strFileName.Replace(" | ", "_").Replace(" ", "-").Replace(":", "_").Replace(",", "-").Replace("..", ".").Replace("/","").Replace(@"\", "").Replace(@"/", "").Replace("&", "-").Replace(">", "-");
            if (bolZnicitTecky)
            {
                s = s.Replace(".", "");
            }
            s = RemoveDiacritism(s);
            return s;

        }
        public static string RemoveDiacritism(string s)
        {
            var stringFormD = s.Normalize(System.Text.NormalizationForm.FormD);
            System.Text.StringBuilder retVal = new System.Text.StringBuilder();
            for (int index = 0; index <= stringFormD.Length - 1; index++)
            {
                if ((System.Globalization.CharUnicodeInfo.GetUnicodeCategory(stringFormD[index]) != System.Globalization.UnicodeCategory.NonSpacingMark))
                    retVal.Append(stringFormD[index]);
            }
            return retVal.ToString().Normalize(System.Text.NormalizationForm.FormC);
        }

        public static string ConvertToSafeFileName(string name,int maxlength=50)
        {

            // first trim the raw string
            string safe = RemoveDiacritism(name.Trim());
            
            // replace spaces with hyphens
            safe = safe.Replace(" ", "-").Replace("&", "-").ToLower();
            
            // replace any 'double spaces' with singles
            if (safe.IndexOf("--") > -1)
                while (safe.IndexOf("--") > -1)
                    safe = safe.Replace("--", "-");

            // trim out illegal characters
            safe = System.Text.RegularExpressions.Regex.Replace(safe, "[^a-z0-9_.\\-]", "");
            
            // trim the length
            if (safe.Length > maxlength)
                safe = safe.Substring(0, 49);
            
            // clean the beginning and end of the filename

            char[] replace = { '-', '.' };
            safe = safe.TrimStart(replace);
            safe = safe.TrimEnd(replace);
            
            return safe;
        }

        public static void LogError(string message, string username = null, string procname = null)
        {
            Handle_Log_Write("error", message, username, procname);
        }
        public static void LogInfo(string message, string username = null, string procname = null)
        {
            Handle_Log_Write("info", message, username, procname);
        }
        private static void Handle_Log_Write(string logname, string message, string username = null, string procname = null)
        {
            var strLogDir = System.IO.Directory.GetCurrentDirectory() + "\\Logs";
            var strPath = string.Format("{0}\\log-{1}-{2}-{3}.log", strLogDir, logname, username, DateTime.Now.ToString("yyyy.MM.dd"));
            try
            {
                System.IO.File.AppendAllLines(strPath, new List<string>() { "", "", "------------------------------", DateTime.Now.ToString() });
                if (procname != null)
                {
                    System.IO.File.AppendAllLines(strPath, new List<string>() { "procname: " + procname });
                }
                if (username != null)
                {
                    System.IO.File.AppendAllLines(strPath, new List<string>() { "username: " + username });
                }
                System.IO.File.AppendAllLines(strPath, new List<string>() { "message: " + message });
            }
            catch
            {
                //nic
            }

        }

        public static FileInfo GetFileInfo(string strFullPath)
        {
            return new FileInfo(strFullPath);
        }


        public static void SaveStream2File(String strDestFullPath, Stream inputStream)
        {

            using (FileStream outputFileStream = new FileStream(strDestFullPath, FileMode.Create))
            {
                inputStream.CopyTo(outputFileStream);
            }


        }

        public static void AppendText2File(String strDestFullPath, string s)
        {
            System.IO.File.AppendAllText(strDestFullPath, s);
        }

        public static List<string> GetFileNamesInDir(string strDir, string strPattern, bool getFullPath)
        {
            DirectoryInfo dir = new DirectoryInfo(strDir);
            var lis = new List<string>();
            foreach (FileInfo file in dir.GetFiles(strPattern))
            {
                if (getFullPath == true)
                {
                    lis.Add(file.FullName);
                }
                else
                {
                    lis.Add(file.Name);
                }


            }

            return lis;
        }


        public static List<string> GetFileListFromDir(string strDir, string strMask, SearchOption so = SearchOption.TopDirectoryOnly, bool bolRetFullPath = false)
        {
            List<string> lis = new List<string>();
            if (!System.IO.Directory.Exists(strDir))
                return lis;

            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(strDir);

            System.IO.FileInfo[] diar1 = di.GetFiles(strMask, so);

            foreach (System.IO.FileInfo dra in diar1)
            {
                if (bolRetFullPath)
                    lis.Add(dra.FullName);
                else
                    lis.Add(dra.Name);
            }
            return lis;
        }

        public static FileInfo GetNewstFileFromDir(string strDir, string strMask, SearchOption so = SearchOption.TopDirectoryOnly)
        {
            if (!System.IO.Directory.Exists(strDir))
                return null;

            List<string> lis = new List<string>();

            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(strDir);
            
            IEnumerable<FileInfo> diar1 = di.GetFiles(strMask, so).OrderByDescending(p => p.CreationTime);
            if (diar1.Count() > 0)
            {
                return diar1.First();
            }

            return null;
        }

        public static void WriteText2File(String strDestFullPath, string s, int encoding_codepage = 0)
        {
            if (encoding_codepage == 0)
            {
                System.IO.File.WriteAllText(strDestFullPath, s);
            }
            else
            {
                System.IO.File.WriteAllText(strDestFullPath, s, Encoding.GetEncoding(encoding_codepage));
            }


        }


        public static string GetContentType(string strFullPath)
        {
            string strExt = GetFileInfo(strFullPath).Extension;

            switch (strExt.ToLower())
            {
                case null:
                case "":
                    return "";
                case ".pdf":
                    return "application/pdf";
                case ".doc":
                    return "application/msword";
                case ".docx":
                    return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                case ".xlsx":
                    return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".gif":
                case ".png":
                case ".bmp":
                    return $"image/{strExt.Substring(1, strExt.Length - 1)}";
                case ".msg":
                case ".eml":
                    return "message/rfc822";
                case ".txt":
                    return "text/plain";
                case ".htm":
                case ".html":
                    return "text/html";
                default:
                    return $"application/{strExt.Replace(".", "")}";
            }
        }

    }
}
