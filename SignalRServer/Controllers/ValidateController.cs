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
        }

        [HttpPost]
        public IActionResult ValidJWT([FromBody] string jwt)
        {
            if (!string.IsNullOrEmpty(jwt))
            {
                (int code, object? result) = jdt.Decoder(jwt, key);
                if (code == 200)
                {
                    return Ok();
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
