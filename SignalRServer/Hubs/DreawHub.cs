using Microsoft.AspNetCore.SignalR;
using SignalRServer.Models;
using Server.Models;
using Newtonsoft.Json;

namespace SignalRServer.Hubs
{
    public class DreawHub : Hub
    {
        public readonly static List<UserModel> conns = new List<UserModel>();
        public readonly static Dictionary<string, string> conns_map = new Dictionary<string, string>();
        
        public async Task BroadcastDraw(string data, Command cmd)
        {
            await Clients.Others.SendAsync("HandleDrawSignal", data, cmd);
        }
    }
}
