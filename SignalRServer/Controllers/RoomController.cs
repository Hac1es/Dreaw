using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Server.Models;
using SignalRServer.Hubs;
using static Umbraco.Core.Constants;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/room")]
    public class RoomController : ControllerBase
    {
        private readonly IHubContext<DreawHub> _hubContext;
        public RoomController(IHubContext<DreawHub> hubContext)
        {
            _hubContext = hubContext;
        }
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

        [HttpPost("getname")]
        public async Task<IActionResult> IsWoring()
        {
            using (var reader = new StreamReader(Request.Body))
            {
                string request = await reader.ReadToEndAsync();
                var result = await FindnameByID(request);
                if (result != null)
                    return Ok(result);
                else
                    return Conflict();
            }
        }

        [HttpPost("getcurrentbmp")]
        public async Task<IActionResult> GetBitmap()
        {
            using (var reader = new StreamReader(Request.Body))
            {
                string roomID = await reader.ReadToEndAsync();
                if (DreawHub.RoomUserMapping.Values.Any(value => value.Item1 == roomID))
                {
                    await _hubContext.Clients.Group(roomID).SendAsync("StopConsumer");
                    var targetConnection = DreawHub.RoomUserMapping
            .Where(c => c.Value.Item1 == roomID) // Lọc các kết nối có roomID khớp
            .Select(c => c.Key) // Chọn ra connectionID (key)
            .FirstOrDefault(); // Lấy kết quả đầu tiên, nếu không có trả về null
                    var data = await DreawHub.GetUserBitmap(targetConnection!, _hubContext);
                    await _hubContext.Clients.Group(roomID).SendAsync("StartConsumer");
                    return Ok(data);
                }
                else
                {
                    var data = await GetRoomData(roomID);
                    if (data != null)
                        return Ok(data);
                    else
                        return NotFound();
                }   
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

        private async Task<string?> FindnameByID(string roomID)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(General.SQLServer))
                {
                    await connection.OpenAsync();

                    // Câu lệnh SQL
                    string query = "SELECT roomName FROM Rooms WHERE roomID = @roomID";

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

        private async Task<string?> GetRoomData(string roomID)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(General.SQLServer))
                {
                    await connection.OpenAsync();

                    // Câu lệnh SQL
                    string query = "SELECT canvasData FROM Rooms WHERE roomID = @roomID";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Thêm tham số
                        command.Parameters.AddWithValue("@roomID", roomID);

                        // Thực thi lệnh SQL
                        object? result = await command.ExecuteScalarAsync();

                        // Kiểm tra và trả về roomName nếu tồn tại
                        if (result != null && result is string canvasData)
                        {
                            return canvasData; // Trả về roomName
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
