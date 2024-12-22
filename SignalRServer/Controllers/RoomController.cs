using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Server.Models;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/room")]
    public class RoomController : ControllerBase
    {
        [HttpPost("exists")]
        public async Task<IActionResult> RoomExists()
        {
            using (var reader = new StreamReader(Request.Body))
            {
                string request = await reader.ReadToEndAsync();
                var result = await CheckRoomExists(request);
                if (result)
                    return Ok();
                else
                    return Conflict();
            }
        }

        [HttpPost("isWorking")]
        public async Task<IActionResult> IsWoring()
        {
            using (var reader = new StreamReader(Request.Body))
            {
                string request = await reader.ReadToEndAsync();
                var result = await CheckRoomActive(request);
                if (result != null)
                    return Ok(result);
                else
                    return Conflict();
            }
        }

        private async Task<bool> CheckRoomExists(string roomID)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(General.SQLServer))
                {
                    await connection.OpenAsync();

                    // Câu lệnh SQL
                    string query = "SELECT 1 FROM Rooms WHERE roomID = @roomID";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Thêm tham số
                        command.Parameters.AddWithValue("@roomID", roomID);

                        // Thực thi lệnh SQL
                        object? result = await command.ExecuteScalarAsync();

                        // Kiểm tra kết quả
                        if (result != null)
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
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            } 
            
        }

        private async Task<string?> CheckRoomActive(string roomID)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(General.SQLServer))
                {
                    await connection.OpenAsync();

                    // Câu lệnh SQL
                    string query = "SELECT roomName FROM Rooms WHERE roomID = @roomID and isUsing = 1";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Thêm tham số
                        command.Parameters.AddWithValue("@roomID", roomID);

                        // Thực thi lệnh SQL
                        object? result = await command.ExecuteScalarAsync();

                        // Kiểm tra và trả về roomName nếu tồn tại
                        if (result != null && result is string roomName)
                        {
                            return roomName; // Trả về roomName
                        }
                        else
                        {
                            return null; // Không tìm thấy, trả về null
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }

        }
    }
}
