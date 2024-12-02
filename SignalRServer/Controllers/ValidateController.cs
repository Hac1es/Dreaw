using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DotNetEnv;
using Newtonsoft.Json.Linq;
using Server.Models;
using System.Text;
using System.Security.Cryptography;

namespace Server.Controllers
{
    [Route("valid")]
    [ApiController]
    public class ValidateController : ControllerBase
    {
        private readonly string key;
        private readonly JDT jdt = new JDT();
        public ValidateController()
        {
            Env.Load();
            key = Env.GetString("SECRET_KEY");
            aes = new AES_things(key);
        }

        private readonly AES_things aes;

        [HttpPost]
        public IActionResult ValidJWT([FromBody] string jwt)
        {
            if (!string.IsNullOrEmpty(jwt))
            {
                (int code, object? result) = jdt.Decoder(jwt, key);
                if (code == 200)
                {
                    string encrpyted_ep = aes.Encrypt("api/hub");
                    return Ok(encrpyted_ep);
                }
                else if (code == 401)
                {
                    return Unauthorized(result as string);
                }
                else
                {
                    return StatusCode(code, $"Failed with the following reason: {result as string}");
                }
                
            }
            return BadRequest("Nothing to valid");
        }

    }
}
