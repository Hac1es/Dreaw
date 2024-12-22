using Microsoft.AspNetCore.SignalR;
using SignalRServer.Models;
using Server.Models;
using System.Collections.Concurrent;
using Umbraco.Core.Collections;
using System.Collections.Generic;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SignalRServer.Hubs
{
    public class DreawHub : Hub
    {
        private static ConcurrentDictionary<string, (string, string, string, string)> RoomUserMapping = new ConcurrentDictionary<string, (string, string, string, string)>();
        private static ConcurrentDictionary<string, string> RoomOwner = new ConcurrentDictionary<string, string>();
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

        public async Task StopConsumer()
        {
            RoomUserMapping.TryGetValue(Context.ConnectionId, out var pair);
            var currentGroup = pair.Item1;
            await Clients.OthersInGroup(currentGroup).SendAsync("StopYourConsumer");
        }
        
        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext()!;
            var userName = httpContext.Request.Query["name"].ToString();
            var userID = httpContext.Request.Query["userID"].ToString();
            var roomID = httpContext.Request.Query["roomID"].ToString();
            var roomName = httpContext.Request.Query["roomname"].ToString();
            var roomOwner = httpContext.Request.Query["ownerID"].ToString();
            if (string.IsNullOrEmpty(userID) || string.IsNullOrEmpty(roomID) || string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(roomName))
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
    }
}
