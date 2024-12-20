using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Server.Models;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/rooms")]
    public class RoomController : ControllerBase
    {
        [HttpPost("validID")]
        [Consumes("text/plain")] // Cấu hình nhận text/plain
        public async Task<IActionResult> CheckID()
        {
            using (var reader = new StreamReader(Request.Body))
            {
                string ID = await reader.ReadToEndAsync();
                if (string.IsNullOrEmpty(ID))
                {
                    return BadRequest();
                }
                var result = await CheckRoomIDSQL(ID);
                if (result)
                {
                    return Ok();
                }
                else
                {
                    return Conflict();
                }
            }
        }

        [HttpPost("validIDreverse")]
        [Consumes("text/plain")] // Cấu hình nhận text/plain
        public async Task<IActionResult> CheckIDReverse()
        {
            using (var reader = new StreamReader(Request.Body))
            {
                string ID = await reader.ReadToEndAsync();
                if (string.IsNullOrEmpty(ID))
                {
                    return BadRequest();
                }
                var result = await CheckRoomIDSQL(ID);
                if (!result)
                {
                    return Ok();
                }
                else
                {
                    return Conflict();
                }
            }
        }

        [HttpPost("isroomactive")]
        [Consumes("text/plain")] // Cấu hình nhận text/plain
        public async Task<IActionResult> IsRoomActive()
        {
            using (var reader = new StreamReader(Request.Body))
            {
                string ID = await reader.ReadToEndAsync();
                if (string.IsNullOrEmpty(ID))
                {
                    return BadRequest();
                }
                var (result, roomName) = await CheckRoomActiveSQL(ID);
                if (result)
                {
                    return Ok(roomName);
                }
                else
                {
                    return Conflict();
                }
            }
        }

        private async Task<bool> CheckRoomIDSQL(string ID)
        {
            string query = "SELECT 1 FROM Rooms WHERE roomID = @ID";
            try
            {
                using (SqlConnection connection = new SqlConnection(General.SQLServer))
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Thêm tham số
                        command.Parameters.AddWithValue("@ID", ID);

                        connection.Open();
                        object? result = await command.ExecuteScalarAsync();
                        return result == null;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        private async Task<(bool, string)> CheckRoomActiveSQL(string ID)
        {
            string query = "SELECT roomName FROM Rooms WHERE roomID = @ID and isUsing = 1";
            try
            {
                using (SqlConnection connection = new SqlConnection(General.SQLServer))
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Thêm tham số
                        command.Parameters.AddWithValue("@ID", ID);

                        await connection.OpenAsync(); // Sử dụng OpenAsync
                        object? result = await command.ExecuteScalarAsync();

                        // Nếu kết quả không null, trả về true và giá trị roomName
                        if (result != null)
                        {
                            return (true, result.ToString()!);
                        }
                        else
                        {
                            return (false, ""); // Không tìm thấy phòng
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return (false, "");
            }
        }
    }
}
