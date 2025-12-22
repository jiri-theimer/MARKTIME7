using ceTe.DynamicPDF;
using ceTe.DynamicPDF.Merger;
using ceTe.DynamicPDF.Merger.Forms;
using ceTe.DynamicPDF.PageElements.Forms;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace UI
{
    public class PdfSupport
    {
        public string PfxPath { get; set; }
        public string PfxPassword { get; set; }
        public string Reason { get; set; }
        public string PdfFieldSubject { get; set; }
        public string Label { get; set; } = "Digitálně podepsáno";
        public float Coordinates_X { get; set; } = 300;
        public float Coordinates_Y { get; set; } = 740;
        public string PodepsatDokument(string strInputPath,string strTargetFullPath)
        {           
            ceTe.DynamicPDF.Document.AddLicense("DPSPROU4223720241231Xap8Eso/OLqTQoAdWV83/EhF3keLURxFeh6eWVIsKRuL5QcYIwkKfrnldyUxzLX17t/Zdk0VJQDF/Ka6byCKNrfL/A");
            

            var podpis = new ceTe.DynamicPDF.PageElements.Forms.Signature("sigfield", this.Coordinates_X, this.Coordinates_Y, 250, 60);
            podpis.Font = Font.Google("Roboto");            
            StringBuilder outputString = new StringBuilder();
            if (!string.IsNullOrEmpty(this.Label))
            {
                outputString.AppendLine(this.Label);
            }
            

            DateTime dt = DateTime.Now;
            DateTime udt = dt.ToUniversalTime();

            string dateStringFormat = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss zzz ");
            outputString.Append(dateStringFormat);
            
            podpis.RightPanel.HideAllText();
            podpis.RightPanel.CustomMessage = outputString.ToString();
            
            if (!string.IsNullOrEmpty(this.Reason))
            {
                podpis.Reason = this.Reason;
            }

            var document = new MergeDocument(strInputPath);
            document.Pages[0].Elements.Add(podpis);

            var certifikat = new X509Certificate2(this.PfxPath, this.PfxPassword, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable);
            document.Sign("sigfield", new Certificate(certifikat));
            //document.Sign("sigfield", new Certificate(this.PfxPath, this.PfxPassword));

            document.Producer = "PDFsharp";
            document.Author = "MARKTIME";
            document.Creator = "MARKTIME";
            document.Title = "Digitálně podepsaný dokument";
            document.Subject = this.PdfFieldSubject;

            document.Draw(strTargetFullPath);

            
            
            return strTargetFullPath;

        }

        public string SaveIsdocAttachmentToTemp(string strPdfPath,string strDestFullPath)
        {
            ceTe.DynamicPDF.Document.AddLicense("DPSPROU4223720241231Xap8Eso/OLqTQoAdWV83/EhF3keLURxFeh6eWVIsKRuL5QcYIwkKfrnldyUxzLX17t/Zdk0VJQDF/Ka6byCKNrfL/A");
            var doc1 = new PdfDocument(strPdfPath);
            var lis = doc1.GetAttachments();
            foreach (var c in lis)
            {
                if (c.Filename.ToLower().Contains("isdoc"))
                {
                    var arr = c.GetData();
                    System.IO.File.WriteAllBytes($"{strDestFullPath}", arr);
                    return strDestFullPath;
                }
                

                
            }

            return null;
        }
    }
}
