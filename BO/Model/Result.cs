using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace BO
{
    public enum ResultEnum
    {
        Failed=0,
        Success=1,
        InfoOnly=2
    }
    public class Result
    {
        private string _message;

        public ResultEnum Flag { get; set; }
        public int pid { get; set; }
        public string Message { 
            get {
                if (this.PreMessage == null)
                {
                    return _message;
                }
                else
                {
                    return this.PreMessage + ": " + _message;
                }
                
            }
            set {
                 _message= value;
            }
        }
        public string PreMessage { get; set; }
        public Result(bool bolError, string strMessage= null,int pid=0)
        {
            if (bolError)
            {
                this.Flag = ResultEnum.Failed;
            }
            else
            {
                this.Flag = ResultEnum.Success;
            }
            this.pid = pid;
            _message = strMessage;
        }

        public bool issuccess
        {
            get
            {
                if (this.Flag == ResultEnum.Success) return true;

                return false;
            }
        }
        
    }
}
