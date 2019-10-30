using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace EduChatAPI.Hubs
{
    public class ChatHub : Hub
    {

        public async Task SendChatMessage(int senderid, string groupToSendTo, int messageId)
        {
            await Clients.All.SendAsync("BROADCAST", "RECIEVED");
            // Client calls SendMessage with senderid, the id of the group to send to and the id of the message
            await Clients.Group(groupToSendTo).SendAsync("ChatMessageRecieved", groupToSendTo, senderid, messageId);
            //All clients in that group listen for the MessageRecieved event with the senderid and the messageid
        }

       
        

        public async Task SubscribeToGroup(string groupId)
        {
            //Temporarily adds client to group subscription
            await Groups.AddToGroupAsync(Context.ConnectionId, groupId);
            await Clients.All.SendAsync("BROADCAST", "SUBJECT SUBSCRIBED");
        }
        public async Task RemoveFromGroup(string groupId)
        {
            //Removes client from group subscription
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupId);
        }

        
       
    }
}
