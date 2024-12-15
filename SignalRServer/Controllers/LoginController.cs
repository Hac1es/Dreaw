using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Server.Models;
using SignalRServer.Models;
using System.Data.SqlClient;

namespace Server.Controllers
{
    [ApiController]
    public class LoginController : ControllerBase
    {
        [Route("api/login")]
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] UserModel request)
        {
            if (request == null)
            {
                return BadRequest("Invalid request");
            }
            string email = request.Email;
            string password = request.Password!;
            var result = await LoginQuery(email, password);
            if (result)
            {
                return Ok();
            }
            else
            {
                return Unauthorized();
            }
        }

        private async Task<bool> LoginQuery(string email, string password)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(General.SQLServer))
                {
                    connection.Open();
                    string query = "SELECT COUNT(1) FROM Users WHERE email = @Email AND password_hash = @PasswordHash";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Email", email);
                        command.Parameters.AddWithValue("@PasswordHash", HashPassword(password));
                        Console.WriteLine(HashPassword(password));
                        var count = await command.ExecuteScalarAsync();
                        if ((int)count! == 1)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
            catch(Exception ex) 
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        private string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(password);
                byte[] hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
}
