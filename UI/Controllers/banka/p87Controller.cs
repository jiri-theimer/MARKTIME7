using Microsoft.AspNetCore.Mvc;
using UI.Models;

namespace UI.Controllers
{
    /// <summary>Bankovní platby načtené z notifikačních e-mailů banky (p87BankTransaction): přehled, ruční přiřazení k dokladu, ignorování.</summary>
    public class p87Controller : BaseController
    {
        private const int _MaxRows = 300;

        public IActionResult Index(int flag = 0)
        {
            if (!Factory.CurrentUser.TestPermission(BO.PermValEnum.GR_P91_Owner))
            {
                return this.StopPage(false, "Nemáte oprávnění k bankovním platbám.");
            }
            var v = new p87ViewModel() { StatusFlag = flag };
            RefreshState(v);
            return View(v);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(p87ViewModel v)
        {
            if (!Factory.CurrentUser.TestPermission(BO.PermValEnum.GR_P91_Owner))
            {
                return this.StopPage(false, "Nemáte oprávnění k bankovním platbám.");
            }

            switch (v.oper)
            {
                case "run":
                    Factory.o42ImapRuleBL.Run_ImapRobot();
                    this.AddMessageTranslated("Načtení e-mailů z banky je dokončeno.", "info");
                    break;
                case "assign":
                    Handle_Assign(v);
                    break;
                case "ignore":
                    Factory.p87BankTransactionBL.Ignore(v.p87id);
                    break;
                case "reopen":
                    Factory.p87BankTransactionBL.Reopen(v.p87id);
                    break;
            }

            ModelState.Clear();
            v.oper = null; v.p87id = 0; v.code = null;
            RefreshState(v);
            return View(v);
        }

        /// <summary>Text e-mailu, ze kterého transakce pochází.</summary>
        public IActionResult MailText(int p87id)
        {
            if (!Factory.CurrentUser.TestPermission(BO.PermValEnum.GR_P91_Owner))
            {
                return this.StopPage(false, "Nemáte oprávnění k bankovním platbám.");
            }
            var rec = Factory.p87BankTransactionBL.Load(p87id);
            var recO27 = (rec == null || rec.o27ID == 0 ? null : Factory.o27AttachmentBL.Load(rec.o27ID));
            if (recO27 == null)
            {
                return this.StopPage(false, "E-mail k této transakci už není k dispozici.");
            }
            string s = recO27.o27MailSubject + "\n" + recO27.o27MailSenderAddress + ", " + BO.Code.Bas.ObjectDate2String(recO27.o27MailDateMessage, "dd.MM.yyyy HH:mm") + "\n\n"
                + BL.Code.BankMailParser.GetText(recO27.o27MailBodyText, recO27.o27MailBodyHtml);
            return Content(s, "text/plain; charset=utf-8");
        }

        private void Handle_Assign(p87ViewModel v)
        {
            string strCode = (v.code ?? "").Trim();
            if (strCode == "")
            {
                this.AddMessageTranslated("Zadejte číslo faktury nebo zálohy.");
                return;
            }
            var rec = Factory.p87BankTransactionBL.Load(v.p87id);
            if (rec == null)
            {
                this.AddMessageTranslated("Bankovní transakce nebyla nalezena.");
                return;
            }

            var recP91 = Factory.p91InvoiceBL.LoadByCode(strCode);
            if (recP91 == null && strCode.All(char.IsDigit)) recP91 = Factory.p91InvoiceBL.LoadByNumericCode(strCode);
            var recP90 = Factory.p90ProformaBL.LoadByCode(strCode);
            if (recP90 == null && strCode.All(char.IsDigit)) recP90 = Factory.p90ProformaBL.LoadByNumericCode(strCode);
            if (recP91 != null && recP91.p91IsDraft) recP91 = null;
            if (recP90 != null && recP90.p90IsDraft) recP90 = null;

            if (recP91 == null && recP90 == null)
            {
                this.AddMessageTranslated("Doklad s tímto číslem nebyl nalezen.");
                return;
            }

            bool bolOK;
            if (recP91 != null && !(recP90 != null && rec.p90ID == recP90.pid))
            {
                bolOK = Factory.p87BankTransactionBL.AssignInvoice(v.p87id, recP91.pid);
            }
            else
            {
                bolOK = Factory.p87BankTransactionBL.AssignProforma(v.p87id, recP90.pid);
            }
            if (bolOK)
            {
                this.AddMessageTranslated("Úhrada byla zapsána.", "info");
            }
        }

        private void RefreshState(p87ViewModel v)
        {
            var lis = Factory.p87BankTransactionBL.GetList(v.StatusFlag, _MaxRows);
            v.lisP87 = (lis == null ? new List<BO.p87BankTransaction>() : lis.ToList());
            v.CountUnpaired = (v.StatusFlag == 0 && v.lisP87.Count < _MaxRows ? v.lisP87.Count : Factory.p87BankTransactionBL.GetCountUnpaired());
        }
    }
}
