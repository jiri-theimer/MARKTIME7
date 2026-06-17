

namespace BO
{
    public enum p92TypeFlagENUM
    {
        ClientInvoice = 1,
        CreditNote = 2
    }
    public enum p92QrCodeFlagENUM
    {
        _None=0,
        QrPlatba=1,        
        QrFaktura=2,
        QrPlatbaBezSplatnosti=3,
        QrFakturaBezSplatnosti=4,
        Slovensko=5,
        SlovenskoBezSplatnosti=6
    }
    //Slovenský qr kod:
    //https://api.freebysquare.sk/pay/v1/generate-string?size=400&color=3&transparent=true&amount=1000&dueDate=20240329&variableSymbol=20240666&paymentNote=Fakturujeme objednané služby&iban=CZ8520100000000848398001&beneficiaryName=Marktime Software s.r.o.&

    public class p92InvoiceType:BaseBO
    {
        public int x01ID { get; set; }
        public p92TypeFlagENUM p92TypeFlag { get; set; } = p92TypeFlagENUM.ClientInvoice;
        public string p92Name { get; set; }
        public string p92Code { get; set; }
        public int p93ID { get; set; }
        public int x31ID_Invoice { get; set; }
        public int x31ID_Attachment { get; set; }
        public int x31ID_Letter { get; set; }
       
        public int j27ID { get; set; }
        
        public int p98ID { get; set; }
        public int p83ID { get; set; }
        public int j19ID { get; set; }
        public int b01ID { get; set; }
        public int x38ID { get; set; }
        public int x38ID_Draft { get; set; }
        public int p80ID { get; set; }
        public int p32ID_CreditNote { get; set; }
        public int j61ID { get; set; }
        public int x04ID { get; set; }
        public x15IdEnum x15ID { get; set; }
        public int p92Ordinary { get; set; }

        public string p92ReportConstantPreText1 { get; set; }
        public string p92InvoiceDefaultText1 { get; set; }
        public string p92InvoiceDefaultText2 { get; set; }
        public string p92ReportConstantText { get; set; }

        
        public string p92RepDocName { get; set; }
        public string p92RepDocNumber { get; set; }
        public byte p92FilesTab { get; set; }
        public byte p92RolesTab { get; set; }

        public p92QrCodeFlagENUM p92QrCodeFlag { get; set; }



        public string j27Code { get; }        
        public string j17Name { get; }       
        public string j61Name { get; }        
        public string p93Name { get; }     
        public string x15Name { get; }
        public string p83Name { get; }
      
    }
}
