using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UI.Models;
using UI.Models.Record;

namespace UI.Controllers
{
    public class x28Controller : BaseController
    {
        public IActionResult Record(int pid, bool isclone,string prefix)
        {
            var v = new x28Record() { rec_pid = pid, rec_entity = "x28" };
            v.Rec = new BO.x28EntityField() { x28Flag = BO.x28FlagENUM.UserField };
            if (v.rec_pid > 0)
            {
                v.Rec = Factory.x28EntityFieldBL.Load(v.rec_pid);
               
                if (v.Rec == null)
                {
                    return RecNotFound(v);
                }
                v.ComboX27Name = v.Rec.x27Name;
                v.SelectedJ04IDs = v.Rec.x28NotPublic_j04IDs;                
                if (v.SelectedJ04IDs != null)
                {
                    var mq = new BO.myQueryJ04() { pids = BO.Code.Bas.ConvertString2ListInt(v.SelectedJ04IDs) };
                    v.SelectedJ04Names = string.Join(",", Factory.j04UserRoleBL.GetList(mq).Select(p => p.j04Name));
                }
                v.SelectedJ07IDs = v.Rec.x28NotPublic_j07IDs;
                if (v.SelectedJ07IDs != null)
                {
                    var mq = new BO.myQuery("j07") { pids = BO.Code.Bas.ConvertString2ListInt(v.SelectedJ07IDs) };
                    v.SelectedJ07Names = string.Join(",", Factory.j07PersonPositionBL.GetList(mq).Select(p => p.j07Name));
                }
                if (v.Rec.x28TextboxHeight > 0)
                {
                    v.IsTextArea = true;    //poznámkové pole
                }

            }
            else
            {
                v.Rec.x28IsAllEntityTypes = true;
                v.Rec.x28Flag = BO.x28FlagENUM.UserField;
                if (prefix != null)
                {
                    v.Rec.x28Entity = prefix;
                }
                else
                {
                    v.Rec.x28Entity = "p41";
                }
                
                v.Rec.x24ID = BO.x24IdENUM.tString;
            }
            RefreshState(v);
            if (v.rec_pid > 0)
            {
                var lisSavedX26 = Factory.x28EntityFieldBL.GetList_x26(v.rec_pid).ToList();
                foreach(var c in v.lisX26)
                {
                    if (lisSavedX26.Any(p => p.x26RecTypePid == c.x26RecTypePid))
                    {
                        c.IsChecked = true;
                        c.x26IsEntryRequired = lisSavedX26.Where(p => p.x26RecTypePid == c.x26RecTypePid).First().x26IsEntryRequired;
                    }
                }
            }
            v.Toolbar = new MyToolbarViewModel(v.Rec);
            if (isclone)
            {
                v.MakeClone();
            }

            return ViewTup(v, BO.PermValEnum.GR_Admin);
        }

        private void RefreshState(x28Record v)
        {
            if (v.lisX26 == null)
            {
                v.lisX26 = new List<BO.x26EntityField_Binding>();
                switch (v.Rec.x28Entity)
                {
                    case "p41":
                        var lis1 = Factory.p42ProjectTypeBL.GetList(new BO.myQueryP42());
                        foreach(var c in lis1)
                        {
                            v.lisX26.Add(new BO.x26EntityField_Binding() { EntityTypeName = c.p42Name+" ("+Factory.tra("Typ projektu")+")", x26RecTypePid = c.pid,x26RecTypeEntity="p42" });
                        }
                        break;
                    case "p31":
                        var lis2 = Factory.p34ActivityGroupBL.GetList(new BO.myQueryP34());
                        foreach (var c in lis2)
                        {
                            v.lisX26.Add(new BO.x26EntityField_Binding() { EntityTypeName = c.p34Name + " (" + Factory.tra("Sešit") + ")", x26RecTypePid = c.pid, x26RecTypeEntity="p34" });
                        }
                        break;
                    case "p28":
                        var lis3 = Factory.p29ContactTypeBL.GetList(new BO.myQuery("p29"));
                        foreach (var c in lis3)
                        {
                            v.lisX26.Add(new BO.x26EntityField_Binding() { EntityTypeName = c.p29Name, x26RecTypePid = c.pid, x26RecTypeEntity="p29" });
                        }
                        break;
                    case "j02":
                        var lis4 = Factory.j07PersonPositionBL.GetList(new BO.myQuery("j07"));
                        foreach (var c in lis4)
                        {
                            v.lisX26.Add(new BO.x26EntityField_Binding() { EntityTypeName = c.j07Name, x26RecTypePid = c.pid, x26RecTypeEntity = "j07" });
                        }
                        break;
                    case "p90":
                        //var lis5 = Factory.p89ProformaTypeBL.GetList(new BO.myQuery("p89"));
                        //foreach (var c in lis5)
                        //{
                        //    v.lisX26.Add(new BO.x26EntityField_Binding() { EntityTypeName = c.p89Name + " (" + Factory.tra("Typ zálohy") + ")", x26EntityTypePID = c.pid, x29ID_EntityType = 389 });
                        //}
                        break;
                    case "p91":
                        //var lis6 = Factory.p92InvoiceTypeBL.GetList(new BO.myQuery("p92"));
                        //foreach (var c in lis6)
                        //{
                        //    v.lisX26.Add(new BO.x26EntityField_Binding() { EntityTypeName = c.p92Name + " (" + Factory.tra("Typ faktury") + ")", x26EntityTypePID = c.pid, x29ID_EntityType = 392 });
                        //}
                        break;
                    
                }
            }
            
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(x28Record v,string oper)
        {
            RefreshState(v);
            if (oper == "postback")
            {
                return View(v);
            }
            if (oper == "x28entity")
            {
                v.lisX26 = null;
                RefreshState(v);
                return View(v);
            }
            if (ModelState.IsValid)
            {
                BO.x28EntityField c = new BO.x28EntityField();
                if (v.rec_pid > 0) c = Factory.x28EntityFieldBL.Load(v.rec_pid);
                c.x28Name = v.Rec.x28Name;                
                c.x24ID = v.Rec.x24ID;
                c.x28Entity = v.Rec.x28Entity;
                             
                c.x27ID = v.Rec.x27ID;

                c.x28Ordinary = v.Rec.x28Ordinary;
                c.x28Flag = v.Rec.x28Flag;
                c.x28DataSource = v.Rec.x28DataSource;
                c.x28IsFixedDataSource = v.Rec.x28IsFixedDataSource;
                c.x28IsRequired = v.Rec.x28IsRequired;
                
                c.x28IsAllEntityTypes = v.Rec.x28IsAllEntityTypes;
                c.x28IsPublic = v.Rec.x28IsPublic;
                c.x28NotPublic_j04IDs = v.SelectedJ04IDs;
                c.x28NotPublic_j07IDs = v.SelectedJ07IDs;

                c.x28Grid_Field = v.Rec.x28Grid_Field;
                c.x28Grid_SqlSyntax = v.Rec.x28Grid_SqlSyntax;
                c.x28Grid_SqlFrom = v.Rec.x28Grid_SqlFrom;
                //c.x28Pivot_SelectSql = v.Rec.x28Pivot_SelectSql;
                //c.x28Pivot_GroupBySql = v.Rec.x28Pivot_GroupBySql;
                //c.x28Query_SqlSyntax = v.Rec.x28Query_SqlSyntax;
                //c.x28Query_Field = v.Rec.x28Query_Field;
                //c.x28Query_sqlComboSource = v.Rec.x28Query_sqlComboSource;

                if (v.IsTextArea)
                {
                    c.x28TextboxHeight = 200;
                }
                else
                {
                    c.x28TextboxHeight = 0;
                }
                
                
                c.x28HelpText = v.Rec.x28HelpText;
                if (c.x24ID==BO.x24IdENUM.tDate || c.x24ID == BO.x24IdENUM.tDateTime)
                {
                    c.x28ReminderNotifyBefore = v.Rec.x28ReminderNotifyBefore;
                }
                

                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);

                c.pid = Factory.x28EntityFieldBL.Save(c,v.lisX26);
                if (c.pid > 0)
                {

                    v.SetJavascript_CallOnLoad(c.pid);
                    return View(v);
                }

                this.Notify_RecNotSaved();
            }


            
            return View(v);
        }
    }
}
