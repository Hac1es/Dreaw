using Microsoft.AspNetCore.SignalR;
using SignalRServer.Models;

namespace SignalRServer.Hubs
{
    public class DreawHub : Hub
    {
        public readonly static List<UserModel> conns = new List<UserModel>();
        public readonly static Dictionary<string, string> conns_map = new Dictionary<string, string>();
        
        public async Task SendFlood(string data)
        {
            await Clients.All.SendAsync("ReceiveData", data);
        }
    }
}
