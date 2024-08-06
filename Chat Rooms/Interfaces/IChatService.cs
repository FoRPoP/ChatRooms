using Microsoft.ServiceFabric.Services.Remoting;
using Interfaces.Helpers;

namespace Interfaces
{
    public interface IChatService : IService
    {
        Task<Dictionary<string, ChatData>> GetChatRooms();

        Task<ChatData?> GetChatRoom(string chatRoomId);

        Task<bool> CreateChatRoom(string name, string description);

        Task<Chat?> JoinChatRoom(string chatRoomId, string userId, string connectionId);

        Task<bool> SendMessage(string chatRoomId, string userId, Message message);

        Task<bool> LeaveChatRoom(string chatRoomId, string userId, string connectionId);

        Task<bool> DeleteChatRoom(string chatId);
    }
}
