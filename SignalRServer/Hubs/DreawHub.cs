using Microsoft.AspNetCore.SignalR;
using SignalRServer.Models;
using Server.Models;
using System.Collections.Concurrent;
using Umbraco.Core.Collections;

namespace SignalRServer.Hubs
{
    public class DreawHub : Hub
    {
        //public readonly static List<UserModel> conns = new List<UserModel>();
        //public readonly static Dictionary<string, string> conns_map = new Dictionary<string, string>();
        //Lưu danh sách connection
        //private static ConcurrentHashSet<string> IDList = new ConcurrentHashSet<string>();
        //private static ConcurrentDictionary<string, TaskCompletionSource<bool>> TaskList = new ConcurrentDictionary<string, TaskCompletionSource<bool>>();
        //public static string data = string.Empty;
        //private static readonly object Lock = new object(); // Tạo đối tượng để đồng bộ hóa

        public async Task BroadcastDraw(string data, Command cmd, bool isPreview)
        {
            await Clients.Others.SendAsync("HandleDrawSignal", data, cmd, isPreview);
        }

        public async Task BroadcastMsg(string msg)
        {
            await Clients.Others.SendAsync("HandleMessage", msg);
        }

        /*public override async Task OnConnectedAsync()
        {
            var thisID = Context.ConnectionId;
            IDList.Add(thisID);
            if (IDList.Count > 1)
            {
                var syncCompleted = new TaskCompletionSource<bool>();
                var firstClient = IDList.First();
                TaskList[firstClient] = syncCompleted;
                await Clients.Client(IDList.First()).SendAsync("Sync");
                await syncCompleted.Task;
                await Clients.Caller.SendAsync("SyncFromServer", GetData());
            }
            else
            {
                await Clients.Caller.SendAsync("SyncFromServer", null);
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            IDList.Remove(Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        public void SyncingData(string clientdata)
        {
            lock (Lock)
            {
                if (TaskList.TryGetValue(Context.ConnectionId, out var syncCompleted))
                {
                    syncCompleted.TrySetResult(true);
                    TaskList.TryRemove(Context.ConnectionId, out _);
                }
                data = clientdata;
            }
        }
        public string GetData()
        {
            lock (Lock)
            {
                return data; // Trả về dữ liệu hiện tại
            }
        }*/
    }
}
