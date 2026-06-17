using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UI.Models.a55
{
    public class WebpageLayerEnvironment
    {
        public List<BO.a58RecPageBox> Col1 { get; set; }
        public List<BO.a58RecPageBox> Col2 { get; set; }
        public List<BO.a58RecPageBox> Col3 { get; set; }
        public List<BO.StringPair> States { get; set; }

        public WebpageLayerEnvironment(string strDockState)
        {
            this.Col1 = new List<BO.a58RecPageBox>();
            this.Col2 = new List<BO.a58RecPageBox>();
            this.Col3 = new List<BO.a58RecPageBox>();
            this.States = new List<BO.StringPair>();

            if (string.IsNullOrEmpty(strDockState))
            {
                return;
            }
            strDockState = strDockState.Replace("sort=", "");
            var lis = BO.Code.Bas.ConvertString2List(strDockState, "|");
            for (int i = 0; i < lis.Count; i++)
            {
                var arr = BO.Code.Bas.ConvertString2List(lis[i], "&");
                for (int x = 0; x < arr.Count(); x++)
                {
                    this.States.Add(new BO.StringPair() { Key = (i + 1).ToString(), Value = arr[x] });
                }
            }
        }
    }
}
