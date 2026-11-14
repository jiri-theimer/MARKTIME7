using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UI.Controllers
{
    /// <summary>
    /// Příjem událostí chatu z druhé webové aplikace MARKTIME (UI <-> mobilní MO), viz UI.Code.ChatRelay.
    /// Volá ho server, ne prohlížeč: bez přihlášení, pravost zaručuje obsah zašifrovaný sdíleným klíčem.
    /// </summary>
    [AllowAnonymous]
    public class ChatRelayController : Controller
    {
        [HttpPost]
        [IgnoreAntiforgeryToken]
        [RequestSizeLimit(256 * 1024)]
        public async Task<IActionResult> Event()
        {
            string strToken;
            using (var reader = new System.IO.StreamReader(Request.Body, System.Text.Encoding.UTF8))
            {
                strToken = await reader.ReadToEndAsync();
            }
            if (!UI.Code.ChatRelay.Receive(HttpContext.RequestServices, strToken)) return StatusCode(403);
            return Ok();
        }
    }
}
