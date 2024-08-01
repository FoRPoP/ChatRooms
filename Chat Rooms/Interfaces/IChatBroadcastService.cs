using Interfaces.Helpers;
using Microsoft.ServiceFabric.Services.Remoting;

namespace Interfaces
{
    public interface IChatBroadcastService : IService
    {
        Task JoinChatRoom(string connectionId, string chatRoomId);
        Task LeaveChatRoom(string connectionId, string chatRoomId);
        Task SendMessage(string connectionId, Message message);
    }
}
