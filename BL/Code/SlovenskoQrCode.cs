

namespace BL.Code
{
    public class SlovenskoQrCode
    {
        public async Task<string> LoadQrCode(BO.p91Invoice rec,BO.p86BankAccount recP86,BO.p93InvoiceHeader recP93,bool bolShowMaturity,HttpClient hc = null)
        {

            if (hc == null)
            {
                hc = new HttpClient();
            }
            
            //https://api.freebysquare.sk/pay/v1/generate-string?size=400&color=3&transparent=true&amount=1000.4&currencyCode=CZK&dueDate=20240319&variableSymbol=12345678&paymentNote=Nazdar vole&iban=&beneficiaryName=ČEZ&

            string url = $"https://api.freebysquare.sk/pay/v1/generate-string?size=400&color=3&transparent=true&amount={BO.Code.Bas.GN(rec.p91Amount_Debt)}&currencyCode={rec.j27Code}&variableSymbol={rec.p91Code}&iban={recP86.p86IBAN}";
            if (recP93 !=null && recP93.p93Company != null)
            {
                url += $"&beneficiaryName={BO.Code.Bas.LeftString(recP93.p93Company,25)}";
            }
            if (bolShowMaturity)
            {
                url += $"&dueDate={rec.p91DateMaturity.ToString("yyyyMMdd")}";
            }
            if (rec.p91Text1 != null)
            {
                url += $"&paymentNote={BO.Code.Bas.LeftString(rec.p91Text1,100)}";
            }

            //BO.Code.File.LogInfo($"SK qrcode: {url}");
            

            using (var request = new HttpRequestMessage(new HttpMethod("GET"), url))
            {
                var response = await hc.SendAsync(request);
                var strJson = await response.Content.ReadAsStringAsync();
                try
                {
                    return strJson;

                }
                catch
                {

                    return null;
                }


            }
        }
    }
}
