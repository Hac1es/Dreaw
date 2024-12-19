using Microsoft.AspNetCore.SignalR;
using SignalRServer.Models;
using Server.Models;
using System.Collections.Concurrent;
using Umbraco.Core.Collections;
using System.Collections.Generic;

namespace SignalRServer.Hubs
{
    public class DreawHub : Hub
    {
        private static ConcurrentDictionary<string, (string, string, string)> RoomUserPair = new ConcurrentDictionary<string, (string, string, string)>();
        private static ConcurrentDictionary<string, string> RoomOwner = new ConcurrentDictionary<string, string>();
        public async Task BroadcastDraw(string data, Command cmd, bool isPreview)
        {
            RoomUserPair.TryGetValue(Context.ConnectionId, out var pair);
            var currentGroup = pair.Item1;
            await Clients.OthersInGroup(currentGroup).SendAsync("HandleDrawSignal", data, cmd, isPreview);
        }

        public async Task BroadcastMsg(string msg, string who)
        {
            RoomUserPair.TryGetValue(Context.ConnectionId, out var pair);
            var currentGroup = pair.Item1;
            await Clients.OthersInGroup(currentGroup).SendAsync("HandleMessage", msg, who);
        }

        /*public async Task/*<bool> SaveRoom(string bmp)
        {
            RoomUserPair.TryGetValue(Context.ConnectionId, out var caller);
            //var result = await CheckIfRoomOwner(caller.Item1, caller.Item2);
            //if (result)
            //{
            //    await SavingBitmap(bmp, caller.Item1, DateTime.UtcNow);
            //   return true;
            //}
            //return false;
        }*/
        
        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext()!;
            var Name = httpContext.Request.Query["name"].ToString();
            var userID = httpContext.Request.Query["userID"].ToString();
            var room = httpContext.Request.Query["room"].ToString();
            if (string.IsNullOrEmpty(userID) || string.IsNullOrEmpty(room) || string.IsNullOrEmpty(Name))
            {
                Console.WriteLine("Kết nối không hợp lệ.");
                Context.Abort(); // Đóng kết nối
                return;
            }
            RoomUserPair.TryAdd(Context.ConnectionId, (room, userID, Name));
            await AddToGroup(Name, Context.ConnectionId, room);
            await base.OnConnectedAsync(); 
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            (string, string, string) caller;
            RoomUserPair.TryRemove(Context.ConnectionId, out caller);
            await RemoveFromGroup(caller.Item3, Context.ConnectionId, caller.Item1);
            //await CheckIfRoomOwner(caller.Item1, caller.Item2);
            await base.OnDisconnectedAsync(exception);
        }

        private async Task<bool> CheckIfRoomOwner(string room, string userID)
        {
            await Task.Delay(1000);
            return true;
        }

        private Task SavingBitmap(string bmp, string room, DateTime now)
        {
            return Task.Delay(1000);
        }

        public async Task AddToGroup(string clientName, string clientID, string groupName)
        {
            await Groups.AddToGroupAsync(clientID, groupName);

            await BroadcastMsg($"{clientName} has joined the room.", "");
        }

        public async Task RemoveFromGroup(string clientName, string clientID, string groupName)
        {
            await Groups.RemoveFromGroupAsync(clientID, groupName);

            await BroadcastMsg($"{clientName} has left the room.", "");
        }
    }
}
