using Microsoft.AspNetCore.SignalR;
using SignalRServer.Models;
using Server.Models;
using System.Collections.Concurrent;
using Umbraco.Core.Collections;
using System.Collections.Generic;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.Data.SqlClient;

namespace SignalRServer.Hubs
{
    public class DreawHub : Hub
    {
        public static ConcurrentDictionary<string, (string, string, string, string)> RoomUserMapping = new ConcurrentDictionary<string, (string, string, string, string)>();
        public static ConcurrentDictionary<string, string> RoomOwner = new ConcurrentDictionary<string, string>();
        public static ConcurrentDictionary<string, TaskCompletionSource<string>> _pendingSynchronization = new ConcurrentDictionary<string, TaskCompletionSource<string>>();
        public async Task BroadcastDraw(string data, Command cmd, bool isPreview)
        {
            RoomUserMapping.TryGetValue(Context.ConnectionId, out var pair);
            var currentGroup = pair.Item1;
            await Clients.OthersInGroup(currentGroup).SendAsync("HandleDrawSignal", data, cmd, isPreview);
        }

        public async Task BroadcastMsg(string msg, string who)
        {
            RoomUserMapping.TryGetValue(Context.ConnectionId, out var pair);
            var currentGroup = pair.Item1;
            await Clients.OthersInGroup(currentGroup).SendAsync("HandleMessage", msg, who);
        }

        public async Task SaveBitmap(string Bitmap)
        {
            RoomUserMapping.TryGetValue(Context.ConnectionId, out var pair);
            var currentGroup = pair.Item1;
            Clients.OthersInGroup(currentGroup).SendAsync("StopConsumer").Wait();
            RoomOwner.TryGetValue(currentGroup, out var roomOwner);
            var roomName = pair.Item2;
            await SavetoDB(Bitmap, roomOwner!, currentGroup, roomName);
            Clients.OthersInGroup(currentGroup).SendAsync("StartConsumer").Wait();
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext()!;
            var userName = httpContext.Request.Query["name"].ToString();
            var userID = httpContext.Request.Query["userID"].ToString();
            var roomID = httpContext.Request.Query["roomID"].ToString();
            var roomName = httpContext.Request.Query["roomname"].ToString();
            var roomOwner = httpContext.Request.Query["ownerID"].ToString();
            if (string.IsNullOrEmpty(userID) || string.IsNullOrEmpty(roomID))
            {
                Console.WriteLine("Kết nối không hợp lệ.");
                Context.Abort(); // Đóng kết nối
                return;
            }
            RoomUserMapping.TryAdd(Context.ConnectionId, (roomID, roomName, userID, userName));
            RoomOwner.TryAdd(roomID, roomOwner);
            await AddToGroup(userName, Context.ConnectionId, roomID);
            await base.OnConnectedAsync(); 
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            (string, string, string, string) caller;
            RoomUserMapping.TryGetValue(Context.ConnectionId, out caller);
            await RemoveFromGroup(caller.Item4, Context.ConnectionId, caller.Item1);
            RoomUserMapping.TryRemove(Context.ConnectionId, out _);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task AddToGroup(string clientName, string clientID, string groupID)
        {
            await Groups.AddToGroupAsync(clientID, groupID);
            await BroadcastMsg($"{clientName} has joined the room.", "");
        }

        public async Task RemoveFromGroup(string clientName, string clientID, string groupID)
        {
            BroadcastMsg($"{clientName} has left the room.", "").Wait();
            await Groups.RemoveFromGroupAsync(clientID, groupID);
        }

        private async Task SavetoDB(string Bitmap, string ownerID, string roomID, string roomName)
        {
            try
            {
                using (var connection = new SqlConnection(General.SQLServer))
                {
                    await connection.OpenAsync();

                    // Kiểm tra phòng đã tồn tại chưa
                    string checkQuery = "SELECT 1 FROM Rooms WHERE roomID = @roomID";
                    using (var checkCommand = new SqlCommand(checkQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@roomID", roomID);

                        var exists = await checkCommand.ExecuteScalarAsync() != null;

                        if (exists)
                        {
                            // Nếu phòng đã tồn tại, cập nhật
                            string updateQuery = @"
                            UPDATE Rooms
                            SET 
                                canvasData = @data, 
                                lastModified = GETDATE()
                            WHERE 
                                roomID = @roomID";
                            using (var updateCommand = new SqlCommand(updateQuery, connection))
                            {
                                updateCommand.Parameters.AddWithValue("@data", Bitmap);
                                updateCommand.Parameters.AddWithValue("@roomID", roomID);

                                await updateCommand.ExecuteNonQueryAsync();
                            }
                        }
                        else
                        {
                            // Nếu phòng chưa tồn tại, thêm mới
                            string insertQuery = @"
                            INSERT INTO Rooms (roomID, roomName, lastModified, ownerID, canvasData)
                            VALUES (@roomID, @roomName, GETDATE(), @ownerID, @data)";
                            using (var insertCommand = new SqlCommand(insertQuery, connection))
                            {
                                insertCommand.Parameters.AddWithValue("@roomID", roomID);
                                insertCommand.Parameters.AddWithValue("@roomName", roomName);
                                insertCommand.Parameters.AddWithValue("@ownerID", ownerID);
                                insertCommand.Parameters.AddWithValue("@data", Bitmap);
                                await insertCommand.ExecuteNonQueryAsync();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving room: {ex.Message}");
            }
        }

        public static async Task<string> GetUserBitmap(string connID, IHubContext<DreawHub> hubContext)
        {
            // Tạo một TaskCompletionSource để chờ kết quả
            var tcs = new TaskCompletionSource<string>();

            // Lưu tạm TaskCompletionSource vào dictionary với key là connID
            _pendingSynchronization[connID] = tcs;

            // Gửi yêu cầu đồng bộ tới client qua SignalR
            await hubContext.Clients.Client(connID).SendAsync("RequestSync");

            // Chờ client trả về Bitmap thông qua TaskCompletionSource
            return await tcs.Task;
        }

        public void SendSyncData(string bitmap)
        {
            // Kiểm tra nếu TaskCompletionSource có tồn tại cho client hiện tại
            if (_pendingSynchronization.TryGetValue(Context.ConnectionId, out var tcs))
            {
                // Hoàn thành TaskCompletionSource và trả kết quả (bitmap)
                tcs.SetResult(bitmap);

                // Xóa TaskCompletionSource khỏi dictionary (không cần nữa)
                _pendingSynchronization.Remove(Context.ConnectionId, out var _);
            }
        }
    }
}
