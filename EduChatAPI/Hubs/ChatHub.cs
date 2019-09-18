using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace EduChatAPI.Hubs
{
    public class ChatHub : Hub
    {

        public async Task SendMessage(int senderid, string groupToSendTo, int messageId)
        {
            // Client calls SendMessage with senderid, the id of the group to send to and the id of the message
            await Clients.Group(groupToSendTo).SendAsync("MessageRecieved", groupToSendTo, senderid, messageId);
            //All clients in that group listen for the MessageRecieved event with the senderid and the messageid
        }

        

        public async Task SubscribeToGroup(string groupId)
        {
            //Temporarily adds client to group subscription
            await Groups.AddToGroupAsync(Context.ConnectionId, groupId);
        }
        public async Task RemoveFromGroup(string groupId)
        {
            //Removes client from group subscription
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupId);
        }

        
       
    }
}
