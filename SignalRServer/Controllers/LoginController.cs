using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Server.Models;
using SignalRServer.Models;
using Microsoft.Data.SqlClient;

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
            var (result, userName) = await LoginQuery(email, password);
            if (result)
            {
                return Ok(userName);
            }
            else
            {
                if (userName == "Invalid credentials")
                    return Unauthorized();
                else
                    return Problem();
            }
        }

        private async Task<(bool, string)> LoginQuery(string email, string password)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(General.SQLServer))
                {
                    await connection.OpenAsync();
                    string query = "SELECT name, userID FROM Users WHERE email = @Email AND password_hash = @PasswordHash";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Email", email);
                        command.Parameters.AddWithValue("@PasswordHash", HashPassword(password));

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                string userName = reader["name"].ToString()!;
                                string userID = reader["userID"].ToString()!;
                                // Trả về JSON chứa thông tin người dùng
                                return (true, JsonConvert.SerializeObject(new { name = userName, userID = userID }));
                            }
                            else
                            {
                                // Email hoặc password không đúng
                                return (false, "Invalid credentials");
                            }
                        }
                    }
                }
            }
            catch(Exception ex) 
            {
                Console.WriteLine(ex.Message);
                return (false, "");
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
