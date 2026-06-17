using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Data;

namespace BO.Code
{
    public static class basJson
    {
        private static JsonSerializerOptions GetOptions()
        {
            var options1 = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.All),            //IgnoreReadOnlyProperties = true,                                                                                                    
                WriteIndented = true
            };

            return options1;
        }
        public static string SerializeObject(object c)
        {

            return JsonSerializer.Serialize(c, GetOptions());
        }
        public static string SerializeDataTable(DataTable lis)
        {
            
            return JsonSerializer.Serialize(lis, GetOptions());
        }

        public static string SerializeData(IEnumerable<Object> lis)
        {

            return JsonSerializer.Serialize(lis, GetOptions());
        }

        public static void SerializeData_To_File(IEnumerable<Object> lis, string strDestFullPath)
        {
            string s = SerializeData(lis);
            BO.Code.File.WriteText2File(strDestFullPath, BO.Code.basJson.SerializeData(lis));

        }



        public static List<T> DeserializeList<T>(string json)
        {
            var ret = JsonSerializer.Deserialize<List<T>>(json);

            return ret;
        }
        public static List<T> DeserializeList_From_File<T>(string strFullPath)
        {
            if (!System.IO.File.Exists(strFullPath))
            {
                return null;
            }
            return DeserializeList<T>(BO.Code.File.GetFileContent(strFullPath));
        }

        public static T DeserializeData<T>(string json)
        {
            var ret = JsonSerializer.Deserialize<T>(json);
            
            return ret;
        }

        public static T DeserializeData_From_File<T>(string strFullPath)
        {
            if (!System.IO.File.Exists(strFullPath))
            {
                return default(T);
            }
            return DeserializeData<T>(BO.Code.File.GetFileContent(strFullPath));
            

        }

    }
}
