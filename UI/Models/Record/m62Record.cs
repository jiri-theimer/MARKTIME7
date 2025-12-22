using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UI.Models.Record
{
    public class m62Record:BaseRecordViewModel
    {
        public BO.m62ExchangeRate Rec { get; set; }

        public string ComboJ27Master { get; set; }
        public string ComboJ27Slave { get; set; }
    }
}
