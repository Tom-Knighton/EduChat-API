using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace EduChatAPI.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendTestMessage(string message)
        {
            await Clients.All.SendAsync("RecieveTestMessage", message);
            //Sends the message back to all connected clients
        }
    }
}
