using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Interfaces;
using Interfaces.Helpers;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.AspNetCore.Authorization;

namespace ChatRoomsWeb.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class ChatController : Controller
    {
        private Uri _serviceUri;

        public ChatController()
        {
            _serviceUri = new Uri("fabric:/Chat_Rooms/ChatService");
        }

        [HttpGet]
        [Route("GetUserInfo")]
        public async Task<UserInfo> GetUserInfo(RegionsEnum region)
        {
            IChatService chatService = GetChatService(region.ToString());
            return await chatService.GetUserInfo(User.Identity!.Name!);
        }

        [HttpGet]
        [Route("GetChatRooms")]
        public async Task<Dictionary<string, ChatData>> GetChatRooms(RegionsEnum region)
        {
            IChatService chatService = GetChatService(region.ToString());
            return await chatService.GetChatRooms();
        }

        [HttpGet]
        [Route("GetChatRoom")]
        public async Task<ChatData?> GetChatRoom(string chatRoomId, RegionsEnum region)
        {
            IChatService chatService = GetChatService(region.ToString());
            return await chatService.GetChatRoom(chatRoomId);
        }

        [HttpPost]
        [Route("CreateChatRoom")]
        public async Task<bool> CreateChatRoom(string name, string description, RegionsEnum region)
        {
            IChatService chatService = GetChatService(region.ToString());
            return await chatService.CreateChatRoom(User.Identity!.Name!, name, description, region);
        }

        [HttpPost]
        [Route("JoinChatRoom")]
        public async Task<Chat?> JoinChatRoom(string chatRoomId, string connectionId, RegionsEnum region)
        {
            IChatService chatService = GetChatService(region.ToString());
            return await chatService.JoinChatRoom(chatRoomId, User.Identity!.Name!, connectionId, region);
        }

        [HttpPost]
        [Route("FavouriteChatRoom")]
        public async Task<bool> FavouriteChatRoom(string chatRoomId, RegionsEnum region)
        {
            IChatService chatService = GetChatService(region.ToString());
            return await chatService.FavouriteChatRoom(chatRoomId, User.Identity!.Name!);
        }

        [HttpPost]
        [Route("SendMessage")]
        public async Task<bool> SendMessage(string chatRoomId, Message message, RegionsEnum region)
        {
            IChatService chatService = GetChatService(region.ToString());
            return await chatService.SendMessage(chatRoomId, message, region);
        }

        [HttpPost]
        [Route("LeaveChatRoom")]
        public async Task<bool> LeaveChatRoom(string chatRoomId, string connectionId, RegionsEnum region)
        {
            IChatService chatService = GetChatService(region.ToString());
            return await chatService.LeaveChatRoom(chatRoomId, User.Identity!.Name!, connectionId, region);
        }

        [HttpDelete]
        [Route("DeleteChatRoom")]
        public async Task<bool> DeleteChatRoom(string chatRoomId, RegionsEnum region)
        {
            IChatService chatService = GetChatService(region.ToString());
            return await chatService.DeleteChatRoom(chatRoomId);
        }

        private IChatService GetChatService(string servicePartitionKeyName)
        {
            return ServiceProxy.Create<IChatService>(_serviceUri, new ServicePartitionKey(servicePartitionKeyName));
        }
    }
}
