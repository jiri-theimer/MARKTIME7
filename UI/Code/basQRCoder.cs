using QRCoder;


namespace UI.Code
{
    public static class basQRCoder
    {
        public static byte[] GenerateContactQrPng(string vcard)
        {
            
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


        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, string> _urlQrCache = new();

        //QR kód s URL jako data URI (PNG, base64) – pro <img src>; výsledek se cachuje (URL bývá pevná, např. mobilní aplikace na přihlašovací stránce)
        public static string GetUrlQrDataUri(string url)
        {
            return _urlQrCache.GetOrAdd(url, u =>
            {
                using var qrGenerator = new QRCodeGenerator();
                using var qrData = qrGenerator.CreateQrCode(u, QRCodeGenerator.ECCLevel.M);
                var png = new PngByteQRCode(qrData).GetGraphic(
                    pixelsPerModule: 8,
                    darkColorRgba: new byte[] { 33, 37, 41, 255 },
                    lightColorRgba: new byte[] { 255, 255, 255, 255 },
                    drawQuietZones: true
                );
                return "data:image/png;base64," + System.Convert.ToBase64String(png);
            });
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
