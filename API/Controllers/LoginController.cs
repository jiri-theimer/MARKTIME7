using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Swashbuckle.AspNetCore.Annotations;
using System.Web;

namespace API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        [SwaggerOperation(Summary = "Přihlášení uživatele přes login+heslo", Description = "Vrací JSON Web Token, kterým se ověřujete v metodách služby.")]
        [HttpPost]
        public IActionResult Get(string login, string password)
        {

            try
            {
               
                if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
                {
                    return BadRequest("Chybí zadat uživatelské jméno a/nebo heslo.");
                }

                password = HttpUtility.UrlEncode(password);
                

                var app = new BL.Singleton.RunningApp();               
                var ru = new BO.RunningUser() { j02Login = login };                

                
                var f = new BL.Factory(ru, app, null, null);
                if (f.CurrentUser == null)
                {
                    return BadRequest("Chybné heslo nebo login.");
                }


                var pwdsupp = new BL.Code.PasswordSupport();
                var ret = pwdsupp.VerifyUserPassword(password, login, f.CurrentUser);
                
                if (ret.Flag==BO.ResultEnum.Failed)
                {
                    return BadRequest("Chybné heslo nebo login.");
                }

             

                var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("Toto je super tajný klíč k API v aplikaci MARKTIME"));
                var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
                var token = new JwtSecurityToken(
                    issuer: "MARKTIME",
                    audience: "https://localhost:7003",
                    claims: new List<Claim>() { new Claim("login", login) },    //uložit login do claims tokenu
                    expires: DateTime.Now.AddMinutes(1000),
                    signingCredentials: signinCredentials
                );

                var strToken = new JwtSecurityTokenHandler().WriteToken(token);


               

                return Ok(strToken);
                

            }
            catch(Exception ex)
            {
                return BadRequest($"An error occurred in generating the token: {ex.Message}");
            }
            
        }




    }
}
