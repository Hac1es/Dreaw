using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SignalRServer.Models;
using Microsoft.Data.SqlClient;
using static System.Net.WebRequestMethods;
using System.Xml.Linq;
using Server.Models;
using Umbraco.Core.Models.Membership;
using System.Net.Mail;
using System.Net;

namespace Server.Controllers
{
    [ApiController]
    public class SignupController : ControllerBase
    {
        [Route("api/signup")]
        [HttpPost]
        public async Task<IActionResult> InitializeSignup([FromBody] UserModel request)
        {
            if (request == null)
            {
                return BadRequest("Invalid request");
            }
            string email = request.Email;
            var (success, msg) = TotheMoon(email);
            if (!success)
            {
                if (msg == "Email exists!")
                    return Conflict();
                else return Problem(msg);
            }
            else
            {
                var OTP = GenerateOTP();
                var result = await SendOTPEmail(email, OTP);
                if (!result)
                {
                    return Problem();
                }    
                return Ok(new { Message = "OTP has been sent to your email.", OTP });
            }
        }

        [Route("api/finishsignup")]
        [HttpPost]
        public async Task<IActionResult> FinishSignup([FromBody] UserModel request)
        {
            if (request == null)
            {
                return BadRequest("Invalid request");
            }
            string email = request.Email;
            string username = request.Username!;
            string password = request.Password!;
            var result = await StoreUser(username, email, password);
            if (result)
            {
                return Ok();
            }
            else
            {
                return Problem();
            }
        }

        [Route("api/resendotp")]
        [HttpPost]
        public async Task<IActionResult> ResendOTP([FromBody] UserModel request)
        {
            string email = request.Email;
            var OTP = GenerateOTP();
            var result = await SendOTPEmail(email, OTP);
            if (!result)
            {
                return Problem();
            }
            return Ok(new { Message = "OTP has been sent to your email.", OTP });
        }

        private (bool, string) TotheMoon(string email)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(General.SQLServer))
                {
                    connection.Open();
                    if (IsEmailExists(email, connection))
                    {
                        return (false, "Email exists!");
                    }
                    return (true, "");
                }
            }
            catch (SqlException ex)
            {
                return (false, $"Database error: {ex.Message}");
            }
        }

        private bool IsEmailExists(string email, SqlConnection connection)
        {
            string query = "SELECT COUNT(1) FROM Users WHERE email = @Email";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Email", email);
                int count = (int)command.ExecuteScalar();
                return count > 0;
            }
        }

        private string GenerateOTP()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString(); // Tạo mã OTP 6 chữ số
        }

        private async Task<bool> SendOTPEmail(string email, string otp)
        {
            await Task.Delay(100); // Giới thiệu một độ trễ nhỏ nếu cần
            try
            {
                // Cấu hình SMTP
                SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587)
                {
                    Credentials = new NetworkCredential("23521458@gm.uit.edu.vn", "mnlwwrbfbojbmcza"), // Chuyển thông tin này ra cấu hình
                    EnableSsl = true
                };
                // Cấu hình email
                MailMessage mail = new MailMessage
                {
                    From = new MailAddress("23521458@gm.uit.edu.vn"),
                    Subject = "OTP Verification",
                    Body = $"Your OTP code is: {otp}",
                    IsBodyHtml = true
                };
                mail.To.Add(email);

                // Gửi email
                await smtp.SendMailAsync(mail);
                return true;
            }
            catch
            {
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

        private async Task<bool> StoreUser(string _name, string _email, string _password)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(General.SQLServer))
                {
                    connection.Open();
                    // Lưu thông tin vào cơ sở dữ liệu sau khi OTP khớp
                    string query = @"
                            INSERT INTO Users (userID, name, password_hash, email, isDrawing, avatar)
                            VALUES (NEWID(), @Name, @PasswordHash, @Email, 0, NULL)";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Name", _name);
                        command.Parameters.AddWithValue("@PasswordHash", HashPassword(_password));
                        command.Parameters.AddWithValue("@Email", _email);

                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        if (rowsAffected > 0)
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
    }
}