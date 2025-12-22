using System;
using System.Collections.Generic;
using System.Text;

namespace DL
{
    public class Params4Dapper
    {
        private List<Param4DT> _lis;
        public Params4Dapper()
        {
            _lis = new List<Param4DT>();

        }
        public void Add(string name,object value)
        {
            
            DL.Param4DT c = new DL.Param4DT() { ParName = name, ParValue = value };
            _lis.Add(c);
        }
        public void AddString(string name,string value)
        {
            if (System.String.IsNullOrEmpty(value) || value.TrimEnd() == "")
            {
                value = null;
            }
            DL.Param4DT c = new DL.Param4DT() {ParamType="string", ParName = name, ParValue = String2Db(value) };
            _lis.Add(c);
        }
        public void AddNonBlackColorString(string name,string value)
        {
            if (value== "#000000")
            {
                value = null;
            }
            DL.Param4DT c = new DL.Param4DT() { ParamType = "string", ParName = name, ParValue = String2Db(value) };
            _lis.Add(c);
        }
        public void AddDateTime(string name, DateTime? value)
        {            

            DL.Param4DT c = new DL.Param4DT() { ParamType = "datetime", ParName = name, ParValue = value };
            _lis.Add(c);
        }
        public void AddDouble(string name, double? value)
        {
            //if (value != null && value == 0) value = null;
            DL.Param4DT c = new DL.Param4DT() { ParamType = "double", ParName = name, ParValue = value };
            _lis.Add(c);
        }
        public void AddByte(string name, byte value)
        {
            DL.Param4DT c = new DL.Param4DT() { ParamType = "byte", ParName = name, ParValue = value };
           
            _lis.Add(c);

        }
        public void AddInt(string name, int value,bool bolIsDbKey=false)
        {
            DL.Param4DT c = new DL.Param4DT() { ParamType = "int", ParName = name,ParValue=value };
            if (bolIsDbKey)
            {
                c.ParValue = BO.Code.Bas.TestIntAsDbKey(value);
            }
            _lis.Add(c);
            
        }
        public void AddEnumInt(string name, Enum value, bool bolIsDbKey = false)
        {            
            DL.Param4DT c = new DL.Param4DT() { ParamType = "int", ParName = name, ParValue = Convert.ToInt32(value) };
            if (bolIsDbKey && Convert.ToInt32(value) == 0)
            {
                c.ParValue = null;
            }
            _lis.Add(c);
        }
        public void AddBool(string name, bool value)
        {
            DL.Param4DT c = new DL.Param4DT() { ParamType = "bool", ParName = name, ParValue = value };
            _lis.Add(c);
        }

        private string String2Db(string s)
        {
            if (String.IsNullOrEmpty(s) || s.Trim() == "")
            {
                return null;
            }

            else
            {
                return s;
            }
        }

        public List<Param4DT> getCatalog()
        {
            return _lis;
        }
        public Dapper.DynamicParameters getDynamicDapperPars()
        {
            var pars = new Dapper.DynamicParameters();
            foreach(Param4DT p in _lis)
            {
                switch (p.ParamType)
                {
                    case "string":
                        pars.Add(p.ParName, p.ParValue, System.Data.DbType.String);
                        break;
                    case "datetime":
                        pars.Add(p.ParName, p.ParValue, System.Data.DbType.DateTime);
                        break;
                    case "bool":
                        pars.Add(p.ParName, p.ParValue, System.Data.DbType.Boolean);
                        break;
                    case "double":
                        pars.Add(p.ParName, p.ParValue, System.Data.DbType.Double);
                        break;
                    case "int":
                        pars.Add(p.ParName, p.ParValue, System.Data.DbType.Int32);
                        break;
                    default:
                        pars.Add(p.ParName, p.ParValue);
                        break;
                }
               
            }
           
            return pars;
        }
    }



    public class Param4DT
    {
        public string ParName { get; set; }
        public object ParValue { get; set; }
        public string ParamType { get; set; }   //string, double,key,datetime,bool - nepovinné, používá se pouze u SaveRecord
    }
}
