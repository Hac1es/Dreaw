using Microsoft.AspNetCore.Mvc;
using Server.Models;
using SignalRServer.Models;
using System.Data.SqlClient;
using System.Net.Mail;
using System.Net;
using System.Security.Cryptography;

namespace Server.Controllers
{
    [ApiController]
    public class ForgotpwController : ControllerBase
    {
        [Route("api/forgetpw")]
        [HttpPost]
        public async Task<IActionResult> InitializeForgotpw([FromBody] UserModel request)
        {
            if (request == null)
            {
                return BadRequest("Invalid request");
            }
            string email = request.Email;
            var success = TotheMoon(email);
            if (!success)
            {
                return NotFound();
            }
            else
            {
                var OTP = GenerateVerificationCode();
                var result = await SendVerificationEmail(email, OTP);
                if (!result)
                {
                    return Problem();
                }
                return Ok(new { Message = "OTP has been sent to your email.", OTP });
            }
        }

        [Route("api/updatepw")]
        [HttpPost]
        public async Task<IActionResult> FinishForgotPw([FromBody] UserModel request)
        {
            if (request == null)
            {
                return BadRequest("Invalid request");
            }
            string email = request.Email;
            string password = request.Password!;
            var result = await UpdatePw(email, password);
            if (result)
            {
                return Ok();
            }
            else
            {
                return Problem();
            }
        }

        private string GenerateVerificationCode()
        {
            byte[] randomNumber = new byte[4];
            RandomNumberGenerator.Fill(randomNumber);
            int code = BitConverter.ToInt32(randomNumber, 0) % 1000000;
            return Math.Abs(code).ToString("D6");
        }

        private bool TotheMoon(string email)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(General.SQLServer))
                {
                    connection.Open();
                    if (IsEmailExists(email, connection))
                    {
                        return true;
                    }
                    return true;
                }
            }
            catch
            {
                return false;
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

        private async Task<bool> SendVerificationEmail(string email, string verificationCode)
        {
            try
            {
                SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587)
                {
                    Credentials = new NetworkCredential("23521458@gm.uit.edu.vn", "echenqlyqkecumoq"),
                    EnableSsl = true
                };
                await Task.Delay(100);
                MailMessage mail = new MailMessage
                {
                    From = new MailAddress("23521458@gm.uit.edu.vn"),
                    Subject = "Password Reset Code",
                    Body = $"Your password reset code is: {verificationCode}",
                    IsBodyHtml = true
                };
                mail.To.Add(email);
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

        private async Task<bool> UpdatePw(string _userEmail, string newPassword)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(General.SQLServer))
                {
                    await connection.OpenAsync();
                    string query = "UPDATE Users SET password_hash = @PasswordHash WHERE email = @Email";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@PasswordHash", HashPassword(newPassword));
                        command.Parameters.AddWithValue("@Email", _userEmail);

                        var rowsAffected = await command.ExecuteNonQueryAsync();
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
            catch
            {
                return false;
            }
        }
    }
}
