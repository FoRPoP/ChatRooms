using Microsoft.ServiceFabric.Services.Remoting;
using Interfaces.Helpers;

namespace Interfaces
{
    public interface IChatService : IService
    {
        Task<UserInfo> GetUserInfo(string username);

        Task<Dictionary<string, ChatData>> GetChatRooms();

        Task<ChatData?> GetChatRoom(string chatRoomId);

        Task<bool> CreateChatRoom(string username, string name, string description);

        Task<Chat?> JoinChatRoom(string chatRoomId, string username, string connectionId);

        Task<bool> FavouriteChatRoom(string chatRoomId, string username);

        Task<bool> SendMessage(string chatRoomId, Message message);

        Task<bool> LeaveChatRoom(string chatRoomId, string username, string connectionId);

        Task<bool> DeleteChatRoom(string chatId);
    }
}
