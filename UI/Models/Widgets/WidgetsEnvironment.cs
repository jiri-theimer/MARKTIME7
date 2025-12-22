using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UI.Models.Widgets
{
    public class WidgetsEnvironment
    {
        public List<BO.x55Widget> Col1 { get; set; }
        public List<BO.x55Widget> Col2 { get; set; }
        public List<BO.x55Widget> Col3 { get; set; }
        public List<BO.StringPair> States { get; set; }

        public WidgetsEnvironment(string strDockState)
        {
            this.Col1 = new List<BO.x55Widget>();
            this.Col2 = new List<BO.x55Widget>();
            this.Col3 = new List<BO.x55Widget>();
            this.States = new List<BO.StringPair>();

            if (string.IsNullOrEmpty(strDockState))
            {
                return;
            }
            strDockState = strDockState.Replace("sort=", "");
            var lis = BO.Code.Bas.ConvertString2List(strDockState, "|");
            for(int i = 0; i < lis.Count; i++)
            {
                var arr = BO.Code.Bas.ConvertString2List(lis[i], "&");
                for (int x= 0;x < arr.Count();x++)
                {
                    this.States.Add(new BO.StringPair() { Key = (i + 1).ToString(), Value = arr[x] });
                }
            }
        }
    }
}
