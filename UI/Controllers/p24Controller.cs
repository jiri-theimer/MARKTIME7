using Microsoft.AspNetCore.Mvc;
using UI.Models.Record;
using UI.Models;
using BO.TimeApi;

namespace UI.Controllers
{
    public class p24Controller : BaseController
    {
        public IActionResult Record(int pid, bool isclone)
        {
            if (!Factory.CurrentUser.j04IsModule_p28)
            {
                return this.StopPage(true, "Nemáte přístup do modulu [Kontakty].");
            }
            var v = new p24Record() { rec_pid = pid, rec_entity = "p24" };
            v.IsMailList_IncludeP30 = Factory.CBL.LoadUserParamBool("p24-IsMailList_IncludeP30", true);
            v.Rec = new BO.p24ContactGroup();
            if (v.rec_pid > 0)
            {
                v.Rec = Factory.p24ContactGroupBL.Load(v.rec_pid);
                if (v.Rec == null)
                {
                    return RecNotFound(v);
                }


                v.p28IDs = string.Join(",", Factory.p28ContactBL.GetList(new BO.myQueryP28() { p24id = v.rec_pid }).Select(p => p.pid));
            }
            v.Toolbar = new MyToolbarViewModel(v.Rec);
            if (isclone)
            {
                v.MakeClone();
            }

            RefreshState(v);

            return View(v);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(Models.Record.p24Record v, string pids, string prefix)
        {
            var lisP28IDs = BO.Code.Bas.ConvertString2ListInt(v.p28IDs);
            if (v.PostbackOper == "add" && prefix == "p28")
            {
                lisP28IDs.AddRange(BO.Code.Bas.ConvertString2ListInt(pids));
            }
            if (v.PostbackOper == "add" && prefix == "p29")
            {
                var lis = Factory.p28ContactBL.GetList(new BO.myQueryP28 { IsRecordValid = true, p29id = BO.Code.Bas.InInt(pids) });
                lisP28IDs.AddRange(lis.Select(p => p.pid).ToList());
            }
            if (v.PostbackOper == "remove" && prefix == "p28")
            {
                foreach (int x in BO.Code.Bas.ConvertString2ListInt(pids))
                {
                    lisP28IDs.Remove(x);
                }
            }

            if (v.IsPostback)
            {
                v.p28IDs = string.Join(",", lisP28IDs);
                RefreshState(v);

                if (v.PostbackOper == "maillist")
                {
                    Handle_Integrace_MailList(v);
                    
                }
                if (v.PostbackOper== "odeslat-kontakty")
                {
                    Factory.CBL.SetUserParam("p24-IsMailList_IncludeP30", v.IsMailList_IncludeP30.ToString());
                    Handle_Integrace_OdeslatKontakty(v);
                }
                
                return View(v);
            }

            RefreshState(v);

            if (ModelState.IsValid)
            {
                BO.p24ContactGroup c = new BO.p24ContactGroup();
                if (v.rec_pid > 0) c = Factory.p24ContactGroupBL.Load(v.rec_pid);
                c.p24Name = v.Rec.p24Name;
                c.p24Email = v.Rec.p24Email;

                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);
                List<int> j02ids = BO.Code.Bas.ConvertString2ListInt(v.p28IDs);
                c.pid = Factory.p24ContactGroupBL.Save(c, j02ids);
                if (c.pid > 0)
                {

                    v.SetJavascript_CallOnLoad(c.pid);
                    return View(v);
                }
            }
            this.Notify_RecNotSaved();
            return View(v);

        }

        private void RefreshState(p24Record v)
        {
            string strMyQuery = "pids|list_int|-1";
            if (!string.IsNullOrEmpty(v.p28IDs))
            {
                strMyQuery = $"pids|list_int|{v.p28IDs}";
            }

            v.gridinput = new Views.Shared.Components.myGrid.myGridInput() { entity = "p28Contact", fixedcolumns = "a__p28Contact__p28Name,a__p28Contact__TypKontaktu,a__p28Contact__Adresa1,a__p28Contact__p28RegID,a__p28Contact__p28VatID", myqueryinline = strMyQuery, oncmclick = "", ondblclick = "" };
            v.gridinput.query = new BO.InitMyQuery(Factory.CurrentUser).Load("p28", null, 0, strMyQuery);

            v.lisX07 = Factory.x07IntegrationBL.GetList(new BO.myQuery("x07"));
            v.lisX07 = v.lisX07.Where(p => p.x07Flag == BO.x07FlagEnum.Ecomail || p.x07Flag == BO.x07FlagEnum.SmartEmailing);
            if(v.lisX07.Count()>0 && v.SelectedX07ID == 0)
            {
                v.SelectedX07ID = v.lisX07.First().pid;
            }
            if (v.SelectedX07ID > 0)
            {
                v.RecX07 = Factory.x07IntegrationBL.Load(v.SelectedX07ID);
                
                
            }
            
            if (v.RecX07 != null)
            {
                Handle_Integrace_MailList(v);
            }
        }


        private void Handle_Integrace_MailList(p24Record v)
        {
            if (v.RecX07 == null)
            {
                return;
            }

            var baseAddress = new Uri("https://api2.ecomailapp.cz/");

            using (var httpClient = new HttpClient { BaseAddress = baseAddress })
            {

                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("key", v.RecX07.x07Token);

                var response = httpClient.GetAsync("lists").Result;
                v.lisMailListIntegrace = new List<BO.StringPair>();

                string responseData = response.Content.ReadAsStringAsync().Result;
                var lis = BO.Code.basJson.DeserializeList<BO.Integrace.Ecomail.List>(responseData);
                foreach (var c in lis)
                {
                    var n = new BO.StringPair() { Key = c.id.ToString(), Value = c.name };
                    v.lisMailListIntegrace.Add(n);
                    
                }

               
            }
        }

        private void Handle_Integrace_OdeslatKontakty(p24Record v)
        {
            if (v.RecX07 == null || v.SelectedMailListID==null)
            {
                return;
            }
            var p28ids = BO.Code.Bas.ConvertString2ListInt(v.p28IDs);
            var lisP28 = Factory.p28ContactBL.GetList(new BO.myQueryP28() { pids = p28ids });

            var baseAddress = new Uri("https://api2.ecomailapp.cz/");
            v.MailList_Count_Errs = 0;v.MailList_Count_OKs = 0;

            using (var httpClient = new HttpClient { BaseAddress = baseAddress })
            {

                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("key", v.RecX07.x07Token);
                
                foreach(var recP28 in lisP28)
                {
                    var lisO32_Company = Factory.p28ContactBL.GetList_o32(recP28.pid, 0,0).Where(p => p.o32Value !=null && (p.o33ID == BO.o33FlagEnum.Email || p.o33ID == BO.o33FlagEnum.EmailCC || p.o33ID == BO.o33FlagEnum.EmailBCC));
                    foreach(var recO32 in lisO32_Company)
                    {
                        var c = new BO.Integrace.Ecomail.CreateSubscriber.RootObject();
                        c.subscriber_data = new BO.Integrace.Ecomail.CreateSubscriber.Subscriber_Data() { email = recO32.o32Value,street=recP28.p28Street1,city=recP28.p28City1,zip=recP28.p28PostCode1 };
                        if (recP28.p28IsCompany)
                        {
                            c.subscriber_data.company = recP28.p28CompanyName;
                        }
                        else
                        {
                            c.subscriber_data.surname = recP28.p28LastName;
                            c.subscriber_data.name = recP28.p28FirstName;
                            c.subscriber_data.pretitle = recP28.p28TitleBeforeName;
                        }
                        

                        string ss = BO.Code.basJson.SerializeObject(c);

                        using (var content = new StringContent(ss, System.Text.Encoding.Default, "application/json"))
                        {
                            string strListID = v.SelectedMailListID;
                            using (var response = httpClient.PostAsync($"lists/{strListID}/subscribe", content))
                            {                                
                                string responseData = response.Result.Content.ReadAsStringAsync().Result;
                                try
                                {
                                    var cc = BO.Code.basJson.DeserializeData<BO.Integrace.Ecomail.Subscriber.Subscriber>(responseData);
                                    v.MailList_Count_OKs++;
                                }
                                catch
                                {
                                    v.MailList_Count_Errs++;
                                }
                                
                            }
                        }

                    }
                    if (v.IsMailList_IncludeP30)
                    {
                        var lisP30 = Factory.p28ContactBL.GetList_p30(recP28.pid).Where(p => !p.isclosed);
                        foreach (var recP30 in lisP30)
                        {
                            var recPerson = Factory.p28ContactBL.Load(recP30.p28ID_Person);
                            var lisO32_Person = Factory.p28ContactBL.GetList_o32(recPerson.pid, 0, 0).Where(p => p.o32Value != null && (p.o33ID == BO.o33FlagEnum.Email || p.o33ID == BO.o33FlagEnum.EmailCC || p.o33ID == BO.o33FlagEnum.EmailBCC));
                            foreach (var recO32 in lisO32_Person)
                            {
                                var c = new BO.Integrace.Ecomail.CreateSubscriber.RootObject();
                                c.subscriber_data = new BO.Integrace.Ecomail.CreateSubscriber.Subscriber_Data() { email = recO32.o32Value, name = recPerson.p28FirstName, surname = recPerson.p28LastName, pretitle = recPerson.p28TitleBeforeName };
                                c.subscriber_data.company = recP28.p28CompanyName;

                                string ss = BO.Code.basJson.SerializeObject(c);

                                using (var content = new StringContent(ss, System.Text.Encoding.Default, "application/json"))
                                {
                                    string strListID = v.SelectedMailListID;
                                    using (var response = httpClient.PostAsync($"lists/{strListID}/subscribe", content))
                                    {
                                        string responseData = response.Result.Content.ReadAsStringAsync().Result;
                                        try
                                        {
                                            var cc = BO.Code.basJson.DeserializeData<BO.Integrace.Ecomail.Subscriber.Subscriber>(responseData);
                                            v.MailList_Count_OKs++;
                                        }
                                        catch
                                        {
                                            v.MailList_Count_Errs++;
                                        }
                                    }
                                }

                            }
                        }
                    }
                    
                    
                    
                    
                    
                }
                


            }
        }




    }
}
