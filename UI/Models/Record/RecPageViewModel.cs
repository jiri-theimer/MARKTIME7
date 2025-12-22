using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UI.Views.Shared.Components.myGrid;
using UI.Views.Shared.Components.myPeriod;

namespace UI.Models
{
    public class RecPageViewModel: BaseViewModel
    {
        public BL.Factory Factory;
        public string prefix { get; set; }        
        public int pid { get; set; }
        public string TabName { get; set; }
        
        public int pid_loaded { get; set; }     //pid načtený z user parametrů
        public List<NavTab> NavTabs;

        public string DefaultNavTabUrl { get; set; }
        public string DefaultNavName { get; set; }
        public string DefTab { get; set; }  //výchozí záložka
        public bool IsShowLeftPanel { get; set; }
        public string MenuCode { get; set; }
        public string Go2GridUrl { get; set; }

        public int SearchedPid { get; set; }
        public string SearchedText { get; set; }

        private string _entity { get; set; }        
        public int p91uhrazene { get; set; }
        public string rez { get; set; }

        private bool? _ShowSearch { get; set; }

        public myGridInput gridinput { get; set; }
        public myPeriodViewModel periodinput { get; set; } //fixní filtr v horním pruhu
        public RecordBinQueryViewModel recordbinquery { get; set; } //filtrování podle archivu
        public p31StateQueryViewModel p31statequery { get; set; }   //filtrování podle stavu aktivit v horním pruhu
        public TheGridQueryViewModel TheGridQueryButton { get; set; }   //pojmenovaný filtr
        public void SetGridUrl()
        {
            if (this.pid > 0)
            {
                
                this.Go2GridUrl = new UI.Menu.TheMenuSupport(Factory).GetMainmenuEntityUrl(this.prefix)+ $"&go2pid={this.pid}";
            }


        }
        public int LoadLastUsedPid(string prefix,string rez)
        {
            this.pid_loaded = Factory.CBL.LoadUserParamInt($"recpage-{prefix}-{rez}-pid");
            return this.pid_loaded;
        }


        

     
        public string entity
        {
            get
            {
                if (_entity == null)
                {
                    _entity = BO.Code.Entity.GetEntity(this.prefix);
                }
                return _entity;
            }
        }
       
        

       


    }
}
