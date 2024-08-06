﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Interfaces;
using Interfaces.Helpers;
using Microsoft.ServiceFabric.Services.Client;

namespace ChatRoomsWeb.Controllers
{
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
        [Route("GetChatRooms")]
        public async Task<Dictionary<string, ChatData>> GetChatRooms()
        {
            IChatService chatService = GetChatService();
            return await chatService.GetChatRooms();
        }

        [HttpGet]
        [Route("GetChatRoom")]
        public async Task<ChatData?> GetChatRoom(string chatRoomId)
        {
            IChatService chatService = GetChatService();
            return await chatService.GetChatRoom(chatRoomId);
        }

        [HttpPost]
        [Route("CreateChatRoom")]
        public async Task<bool> CreateChatRoom(string username, string name, string description)
        {
            IChatService chatService = GetChatService();
            return await chatService.CreateChatRoom(username, name, description);
        }

        [HttpPost]
        [Route("JoinChatRoom")]
        public async Task<Chat?> JoinChatRoom(string chatRoomId, string username, string connectionId)
        {
            IChatService chatService = GetChatService();
            return await chatService.JoinChatRoom(chatRoomId, username, connectionId);
        }

        [HttpPost]
        [Route("SendMessage")]
        public async Task<bool> SendMessage(string chatRoomId, Message message)
        {
            IChatService chatService = GetChatService();
            return await chatService.SendMessage(chatRoomId, message);
        }

        [HttpPost]
        [Route("LeaveChatRoom")]
        public async Task<bool> LeaveChatRoom(string chatRoomId, string username, string connectionId)
        {
            IChatService chatService = GetChatService();
            return await chatService.LeaveChatRoom(chatRoomId, username, connectionId);
        }

        [HttpDelete]
        [Route("DeleteChatRoom")]
        public async Task<bool> DeleteChatRoom(string chatRoomId)
        {
            IChatService chatService = GetChatService();
            return await chatService.DeleteChatRoom(chatRoomId);
        }

        private IChatService GetChatService()
        {
            return ServiceProxy.Create<IChatService>(_serviceUri, new ServicePartitionKey(1));
        }
    }
}
