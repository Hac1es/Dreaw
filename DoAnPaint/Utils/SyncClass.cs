using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DoAnPaint.Utils
{
    public class SyncClass
    {
        public async void Send(object data, Command cmd)
        {
            //Do sth
            //RESTful API
            //SignalR
            //ASP.NET API
            //Drawing, Bitmap
            //HubConnection.InvokeAsync("SendFlood", data, cmd);
        }

        public async void Receive()
        {
            //HubConnection.On<object>("Listen", (data)
            //=> {
            //      HandleData(data, command)});
        }
    }
}
