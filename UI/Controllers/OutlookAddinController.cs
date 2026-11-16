using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UI.Code.OutlookAddin;

namespace UI.Controllers
{
    /// <summary>
    /// Doplněk pro Outlook (webový doplněk Office): tlačítko „MARKTIME“ u e-mailu otevře panel (Pane), ze kterého lze e-mail
    /// uložit do FILEBOXu, založit z něj úkol nebo vykázat úkon. Panel je stránka MARKTIME zobrazená uvnitř Outlooku.
    ///   Manifest      – popis doplňku pro Outlook (adresy tohoto serveru); nahrává ho správce Microsoft 365 nebo uživatel
    ///   Pane          – panel; bez přihlašovací cookie, přihlašuje se tokenem (OutlookAddinBearerHandler)
    ///   ConnectStart  – mezistránka okna „Připojit“ → OutlookAddinConnectController (běžné přihlášení MARKTIME, vydá token)
    ///   Me, Push, SaveToFilebox – volá panel s tokenem
    ///   Go            – mezistránka v prohlížeči: otevře MARKTIME s formulářem úkolu / úkonu předvyplněným z e-mailu
    /// E-mail jde stejnou cestou jako po přetažení do prvku FILEBOX: fronta p85Tempbox + parametr mailguid formuláře
    /// (UI.Code.MailRecordSupport, o27AttachmentBL.MoveDropzoneTemp).
    /// Nedědí z BaseController: ten vyžaduje přihlašovací cookie a nepřihlášeného přesměruje na login.
    /// </summary>
    public class OutlookAddinController : Controller
    {
        public const string AddinId = "6f0d3c1e-8a55-4b0e-9b5e-3d6f2a7c41b8";
        public const string AddinVersion = "1.0.0.0";
        private const int MaxMailBytes = 25 * 1024 * 1024;

        private BL.Factory Factory => (BL.Factory)HttpContext.RequestServices.GetService(typeof(BL.Factory));

        // veřejná adresa aplikace včetně případné virtuální složky, bez lomítka na konci
        private string BaseUrl => $"{Request.Scheme}://{Request.Host}{Request.PathBase}".TrimEnd('/');

        [AllowAnonymous]
        public IActionResult Manifest()
        {
            string b = BaseUrl;
            string host = System.Security.SecurityElement.Escape($"{Request.Scheme}://{Request.Host}");
            string pane = System.Security.SecurityElement.Escape($"{b}/OutlookAddin/Pane");
            string icon(int size) => System.Security.SecurityElement.Escape($"{b}/images/outlookaddin/icon-{size}.png");

            string xml = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<OfficeApp xmlns=""http://schemas.microsoft.com/office/appforoffice/1.1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:bt=""http://schemas.microsoft.com/office/officeappbasictypes/1.0"" xmlns:mailappor=""http://schemas.microsoft.com/office/mailappversionoverrides/1.0"" xsi:type=""MailApp"">
  <Id>{AddinId}</Id>
  <Version>{AddinVersion}</Version>
  <ProviderName>MARKTIME Software s.r.o.</ProviderName>
  <DefaultLocale>cs-CZ</DefaultLocale>
  <DisplayName DefaultValue=""MARKTIME"" />
  <Description DefaultValue=""Z e-mailu založí úkol, vykáže úkon nebo ho uloží do FILEBOXu v MARKTIME."" />
  <IconUrl DefaultValue=""{icon(64)}"" />
  <HighResolutionIconUrl DefaultValue=""{icon(128)}"" />
  <SupportUrl DefaultValue=""https://www.marktime.cz"" />
  <AppDomains>
    <AppDomain>{host}</AppDomain>
  </AppDomains>
  <Hosts>
    <Host Name=""Mailbox"" />
  </Hosts>
  <Requirements>
    <Sets>
      <Set Name=""Mailbox"" MinVersion=""1.4"" />
    </Sets>
  </Requirements>
  <FormSettings>
    <Form xsi:type=""ItemRead"">
      <DesktopSettings>
        <SourceLocation DefaultValue=""{pane}"" />
        <RequestedHeight>250</RequestedHeight>
      </DesktopSettings>
    </Form>
  </FormSettings>
  <Permissions>ReadItem</Permissions>
  <Rule xsi:type=""RuleCollection"" Mode=""Or"">
    <Rule xsi:type=""ItemIs"" ItemType=""Message"" FormType=""Read"" />
  </Rule>
  <DisableEntityHighlighting>false</DisableEntityHighlighting>
  <VersionOverrides xmlns=""http://schemas.microsoft.com/office/mailappversionoverrides"" xsi:type=""VersionOverridesV1_0"">
    <Requirements>
      <bt:Sets DefaultMinVersion=""1.4"">
        <bt:Set Name=""Mailbox"" />
      </bt:Sets>
    </Requirements>
    <Hosts>
      <Host xsi:type=""MailHost"">
        <DesktopFormFactor>
          <ExtensionPoint xsi:type=""MessageReadCommandSurface"">
            <OfficeTab id=""TabDefault"">
              <Group id=""mtGroup"">
                <Label resid=""mtGroupLabel"" />
                <Control xsi:type=""Button"" id=""mtPaneButton"">
                  <Label resid=""mtButtonLabel"" />
                  <Supertip>
                    <Title resid=""mtButtonLabel"" />
                    <Description resid=""mtButtonTip"" />
                  </Supertip>
                  <Icon>
                    <bt:Image size=""16"" resid=""mtIcon16"" />
                    <bt:Image size=""32"" resid=""mtIcon32"" />
                    <bt:Image size=""80"" resid=""mtIcon80"" />
                  </Icon>
                  <Action xsi:type=""ShowTaskpane"">
                    <SourceLocation resid=""mtPaneUrl"" />
                  </Action>
                </Control>
              </Group>
            </OfficeTab>
          </ExtensionPoint>
        </DesktopFormFactor>
      </Host>
    </Hosts>
    <Resources>
      <bt:Images>
        <bt:Image id=""mtIcon16"" DefaultValue=""{icon(16)}"" />
        <bt:Image id=""mtIcon32"" DefaultValue=""{icon(32)}"" />
        <bt:Image id=""mtIcon80"" DefaultValue=""{icon(80)}"" />
      </bt:Images>
      <bt:Urls>
        <bt:Url id=""mtPaneUrl"" DefaultValue=""{pane}"" />
      </bt:Urls>
      <bt:ShortStrings>
        <bt:String id=""mtGroupLabel"" DefaultValue=""MARKTIME"" />
        <bt:String id=""mtButtonLabel"" DefaultValue=""MARKTIME"" />
      </bt:ShortStrings>
      <bt:LongStrings>
        <bt:String id=""mtButtonTip"" DefaultValue=""Založit úkol, vykázat úkon nebo uložit e-mail do MARKTIME."" />
      </bt:LongStrings>
    </Resources>
  </VersionOverrides>
</OfficeApp>";
            return Content(xml, "text/xml", Encoding.UTF8);
        }

        [AllowAnonymous]
        public IActionResult Pane()
        {
            return View();
        }

        // Okno „Připojit“ otevírá Outlook z cizího webu – přihlašovací cookie (SameSite=Strict) by se na první požadavek neposlala.
        // Odsud už prohlížeč pokračuje jako z MARKTIME.
        [AllowAnonymous]
        public IActionResult ConnectStart()
        {
            return View("Forward", Url.Content("~/OutlookAddinConnect/Index"));
        }

        // Prohlížeč otevřený z panelu: hlavní stránka MARKTIME s oknem formuláře (adresa okna #w=…, viz wwwroot/js/mtmodal.js).
        [AllowAnonymous]
        public IActionResult Go(string target, string guid)
        {
            if ((target != "p56" && target != "p31") || !UI.Code.MailRecordSupport.IsTempGuid(guid))
            {
                return View("Forward", Url.Content("~/"));
            }
            string form = Url.Content($"~/{target}/Record") + "?pid=0&mailguid=" + guid;    //jen znaky, které encodeURI nemění (mtmodal.js: decodeURI(atob(…)))
            string strB64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(form)).Replace('+', '-').Replace('/', '_').TrimEnd('=');
            return View("Forward", Url.Content("~/") + "#w=" + strB64);
        }

        [Authorize(AuthenticationSchemes = OutlookAddinBearerHandler.SchemeName)]
        public IActionResult Me()
        {
            var u = Factory.CurrentUser;
            return Json(new { name = u.FullNameAsc, login = u.j02Login, canTask = u.j04IsModule_p56, canUkon = u.j04IsModule_p31 });
        }

        public class PushInput
        {
            /// <summary>Celý e-mail ve formátu EML, Base64 (Office.js getAsFileAsync). Starší Outlook ho neumí – pak se pošlou jen pole níže.</summary>
            public string eml { get; set; }
            public string subject { get; set; }
            public string body { get; set; }
            public string fromName { get; set; }
            public string fromAddress { get; set; }
            public DateTime? date { get; set; }
        }

        /// <summary>E-mail z Outlooku do fronty přetažených souborů (stejný tvar jako DropzoneController.UploadFiles). Vrací guid fronty.</summary>
        [Authorize(AuthenticationSchemes = OutlookAddinBearerHandler.SchemeName)]
        [HttpPost]
        [IgnoreAntiforgeryToken]
        [RequestSizeLimit(40 * 1024 * 1024)]
        public IActionResult Push([FromBody] PushInput v)
        {
            if (v == null) return BadRequest(new { message = "Chybí data e-mailu." });
            byte[] data;
            try
            {
                data = string.IsNullOrEmpty(v.eml) ? BuildEml(v) : Convert.FromBase64String(v.eml);
            }
            catch
            {
                return BadRequest(new { message = "E-mail se nepodařilo přečíst." });
            }
            if (data.Length == 0 || data.Length > MaxMailBytes) return BadRequest(new { message = "E-mail je prázdný nebo příliš velký (limit 25 MB)." });

            var f = Factory;
            string strGuid = BO.Code.Bas.GetGuid();
            string strUid = Guid.NewGuid().ToString();
            string strTempName = strUid + ".eml";
            if (!System.IO.Directory.Exists(f.TempFolder)) System.IO.Directory.CreateDirectory(f.TempFolder);
            System.IO.File.WriteAllBytes(System.IO.Path.Combine(f.TempFolder, strTempName), data);

            var rec = new BO.p85Tempbox()
            {
                p85GUID = strGuid, p85Prefix = "eml", p85FreeText01 = SafeFileName(v.subject) + ".eml", p85FreeText02 = "message/rfc822",
                p85FreeText03 = strTempName, p85FreeText04 = strUid, p85FreeText05 = v.subject, p85FreeNumber01 = data.Length
            };
            if (f.p85TempboxBL.Save(rec) <= 0) return BadRequest(new { message = "E-mail se nepodařilo uložit do fronty." });
            return Json(new { guid = strGuid });
        }

        [Authorize(AuthenticationSchemes = OutlookAddinBearerHandler.SchemeName)]
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public IActionResult SaveToFilebox(string guid)
        {
            if (!UI.Code.MailRecordSupport.IsTempGuid(guid)) return BadRequest(new { message = "Neplatná fronta." });
            if (!Factory.o27AttachmentBL.SaveDropzoneFromTemp(guid, null, 0)) return BadRequest(new { message = "E-mail se nepodařilo uložit do FILEBOXu." });
            return Json(new { ok = true });
        }

        private static string SafeFileName(string s)
        {
            s = string.IsNullOrWhiteSpace(s) ? "email" : s.Trim();
            foreach (char c in System.IO.Path.GetInvalidFileNameChars()) s = s.Replace(c, '_');
            s = s.Replace('/', '_').Replace('\\', '_').Replace(':', '_').Replace('*', '_').Replace('?', '_').Replace('"', '_').Replace('<', '_').Replace('>', '_').Replace('|', '_');
            return s.Length > 100 ? s.Substring(0, 100) : s;
        }

        // Starší Outlook neumí vrátit celý e-mail: z předmětu, odesílatele, data a textu se složí jednoduchý .eml,
        // aby zbytek (čtení e-mailu, příloha záznamu) běžel stejnou cestou.
        private static byte[] BuildEml(PushInput v)
        {
            string enc(string s) => "=?utf-8?B?" + Convert.ToBase64String(Encoding.UTF8.GetBytes(s ?? "")) + "?=";
            var sb = new StringBuilder();
            string strAddr = (v.fromAddress ?? "").Replace("<", "").Replace(">", "").Replace("\r", "").Replace("\n", "").Trim();
            if (strAddr != "")
            {
                sb.Append("From: ").Append(string.IsNullOrWhiteSpace(v.fromName) ? "" : enc(v.fromName.Trim()) + " ").Append('<').Append(strAddr).Append(">\r\n");
            }
            sb.Append("Subject: ").Append(enc((v.subject ?? "").Replace("\r", " ").Replace("\n", " "))).Append("\r\n");
            sb.Append("Date: ").Append((v.date ?? DateTime.Now).ToUniversalTime().ToString("r")).Append("\r\n");
            sb.Append("MIME-Version: 1.0\r\nContent-Type: text/plain; charset=utf-8\r\nContent-Transfer-Encoding: base64\r\n\r\n");
            sb.Append(Convert.ToBase64String(Encoding.UTF8.GetBytes(v.body ?? ""), Base64FormattingOptions.InsertLineBreaks)).Append("\r\n");
            return Encoding.ASCII.GetBytes(sb.ToString());
        }
    }
}
