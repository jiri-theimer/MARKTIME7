using System;

namespace BO
{
    public class p93InvoiceHeader:BaseBO
    {
        public int x01ID { get; set; }
        public int j27ID_Domestic { get; set; }
        public string p93Name { get; set; }
        public string p93Company { get; set; }
        public string p93City { get; set; }
        public string p93Street { get; set; }
        public string p93Zip { get; set; }
        public string p93RegID { get; set; }
        public string p93VatID { get; set; }
        public string p93Contact { get; set; }
        public string p93Registration { get; set; }
        public string p93Referent { get; set; }
        public string p93Signature { get; set; }
        public string p93FreeText01 { get; set; }
        public string p93FreeText02 { get; set; }
        public string p93FreeText03 { get; set; }
        public string p93FreeText04 { get; set; }
        public string p93FreeText05 { get; set; }
        public string p93LogoFile { get; set; }
        public string p93Country { get; set; }
        public string p93CountryCode { get; set; }
        public string p93Email { get; set; }
        public string p93SignatureFile { get; set; }
        public string p93ICDPH_SK { get; set; }
        public string p93PfxCertificate { get; set; }
        public string p93PfxPassword { get; set; }
    }
}
