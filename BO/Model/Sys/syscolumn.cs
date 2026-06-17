using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BO.Sys
{
    public class syscolumn
    {
        public int id { get; set; }
        public string name { get; set; }
        public int xtype { get; set; }
        public int typestat { get; set; }
        public int xusertype { get; set; }
        public int length { get; set; }
        public int xprec { get; set; }
        public int xscale { get; set; }
        public int colid { get; set; }
        public int xoffset { get; set; }
        public int bitpos { get; set; }
        public int reserved { get; set; }
        public int colstat { get; set; }
        public int cdefault { get; set; }       //ID constaint pro DEFAULT VALUE
        public int domain { get; set; }
        public int number { get; set; }
        public int colorder { get; set; }
        public int autoval { get; set; }
        public int offset { get; set; }
        public int collationid { get; set; }
        public int language { get; set; }
        public int status { get; set; }
        public int type { get; set; }
        public int usertype { get; set; }
        public int printfmt { get; set; }
        public int prec { get; set; }
        public int scale { get; set; }
        public bool iscomputed { get; set; }
        public bool isoutparam { get; set; }
        public bool isnullable { get; set; }
        public string collation { get; set; }
        public byte[] tdscollation { get; set; }
        public string type_name { get; set; }
        public string tablename { get; set; }
        public string formula { get; set; }

        public bool isidentity
        {
            get
            {
                if (this.status == 128)
                {
                    return true;
                }
                return false;
            }
        }
        
    }
}
