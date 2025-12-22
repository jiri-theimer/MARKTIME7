using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BO
{
    public class j08CreatePermission
    {
        [Key]
        public int j08ID { get; set; }

        public int j11ID { get; set; }
        public int j02ID { get; set; }
        public int j04ID { get; set; }
        public bool j08IsAllUsers { get; set; }
        public int j08RecordPid { get; set; }
        public string j08RecordEntity { get; set; }
    }
}
