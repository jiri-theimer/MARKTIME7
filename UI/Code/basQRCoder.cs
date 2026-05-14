using QRCoder;


namespace UI.Code
{
    public static class basQRCoder
    {
        public static byte[] GenerateContactQrPng(string firstname,string lastname,string org,string tel,string email,string url)
        {
            string vcard = $"""
BEGIN:VCARD
VERSION:3.0
N:{lastname};{firstname};;;
FN:{firstname} {lastname}
ORG:{org}
TEL;TYPE=CELL:{tel}
EMAIL:{email}
URL:{url}
END:VCARD
""";

            using var qrGenerator = new QRCodeGenerator();
            using var qrData = qrGenerator.CreateQrCode(vcard, QRCodeGenerator.ECCLevel.M);
            
            var pngQrCode = new PngByteQRCode(qrData);

            // Modrá #0066CC, pozadí bílé
            return pngQrCode.GetGraphic(
                pixelsPerModule: 4,
                darkColorRgba: new byte[] { 0, 102, 204, 255 },
                lightColorRgba: new byte[] { 255, 255, 255, 255 },
                drawQuietZones: false
            );
        }


        public static string GenerateVcf(string firstname, string lastname, string org, string tel, string email, string url)
        {
            string vcard = $"""
BEGIN:VCARD
VERSION:3.0
N:{lastname};{firstname};;;
FN:{firstname} {lastname}
ORG:{org}
TEL;TYPE=CELL:{tel}
EMAIL:{email}
URL:{url}
END:VCARD
""";

            return vcard;
        }
    }
}
