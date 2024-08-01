using Interfaces.Helpers;
using Microsoft.AspNetCore.SignalR;

namespace ChatBroadcastService
{
    public class ChatBroadcastServiceHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            await Clients.Caller.SendAsync("ReceiveConnectionId", Context.ConnectionId);
            await base.OnConnectedAsync();
        }
    }
}
