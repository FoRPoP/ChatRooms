using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Data;
using Interfaces;
using Interfaces.Helpers;
using Microsoft.ServiceFabric.Data.Collections;
using System.Fabric;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Client;
using System.Runtime.InteropServices;
using Microsoft.ServiceFabric.Services.Client;

namespace ChatService
{
    /// <summary>
    /// The FabricRuntime creates an instance of this class for each service type instance.
    /// </summary>
    internal sealed class ChatService : StatefulService, IChatService
    {
        private ServiceProxyFactory _proxyFactory;
        private Uri _chatBroadcastServiceUri;

        public ChatService(StatefulServiceContext context)
            : base(context)
        {
            _proxyFactory = new ServiceProxyFactory(c => { return new FabricTransportServiceRemotingClientFactory(); });
            _chatBroadcastServiceUri = new Uri("fabric:/Chat_Rooms/ChatBroadcastService");
        }

        public async Task<UserInfo> GetUserInfo(string username)
        {
            IReliableDictionary<string, UserInfo> _userInfos = await StateManager.GetOrAddAsync<IReliableDictionary<string, UserInfo>>("userInfos");

            UserInfo userInfo;
            using (ITransaction tx = StateManager.CreateTransaction())
            {
                userInfo = await _userInfos.GetOrAddAsync(tx, username, _ => new UserInfo());
                await tx.CommitAsync();
            }
            return userInfo;
        }

        public async Task<Dictionary<string, ChatData>> GetChatRooms()
        {
            IReliableDictionary<string, Chat> _chatRooms = await StateManager.GetOrAddAsync<IReliableDictionary<string, Chat>>("chatRooms");

            Dictionary<string, ChatData> chats = new();
            using (ITransaction tx = StateManager.CreateTransaction())
            {
                var enumerable = await _chatRooms.CreateEnumerableAsync(tx);
                var enumerator = enumerable.GetAsyncEnumerator();

                while (await enumerator.MoveNextAsync(CancellationToken.None))
                {
                    var kvp = enumerator.Current;
                    chats.Add(kvp.Key, kvp.Value.ChatData);
                }
            }

            return chats;
        }

        public async Task<ChatData?> GetChatRoom(string chatRoomId)
        {
            IReliableDictionary<string, Chat> _chatRooms = await StateManager.GetOrAddAsync<IReliableDictionary<string, Chat>>("chatRooms");

            ChatData? chatData;
            using (ITransaction tx = StateManager.CreateTransaction())
            {
                ConditionalValue<Chat> chat = await _chatRooms.TryGetValueAsync(tx, chatRoomId);
                if (chat.HasValue)
                {
                    chatData = chat.Value.ChatData;
                }
                else
                {
                    chatData = null;
                }
                await tx.CommitAsync();
            }

            return chatData;
        }

        public async Task<bool> CreateChatRoom(string ownerUsername, string name, string description, RegionsEnum region)
        {
            IReliableDictionary<string, Chat> _chatRooms = await StateManager.GetOrAddAsync<IReliableDictionary<string, Chat>>("chatRooms");
            IReliableDictionary<string, UserInfo> _userInfos = await StateManager.GetOrAddAsync<IReliableDictionary<string, UserInfo>>("userInfos");
            IReliableDictionary<string, int> _IDs = await StateManager.GetOrAddAsync<IReliableDictionary<string, int>>("IDs");

            bool result;
            using (ITransaction tx = StateManager.CreateTransaction())
            {
                int nextChatRoomId = await _IDs.AddOrUpdateAsync(tx, "ChatRoomIDs", 0, (_, value) => value+1);
                ChatData chatData = new(nextChatRoomId.ToString(), name, description, ownerUsername, region);
                Chat chat = new(chatData);

                result = await _chatRooms.TryAddAsync(tx, chatData.ID, chat);

                UserInfo userInfo = await _userInfos.GetOrAddAsync(tx, ownerUsername, _ => new UserInfo());
                userInfo.CreatedChatsIds.Add(chatData.ID);
                await _userInfos.AddOrUpdateAsync(tx, ownerUsername, userInfo, (_, __) => userInfo);

                await tx.CommitAsync();
            }

            return result;
        }

        public async Task<Chat?> JoinChatRoom(string chatRoomId, string username, string connectionId, RegionsEnum region)
        {
            IReliableDictionary<string, Chat> _chatRooms = await StateManager.GetOrAddAsync<IReliableDictionary<string, Chat>>("chatRooms");
            ConditionalValue<Chat> chat;

            using (ITransaction tx = StateManager.CreateTransaction())
            {
                chat = await _chatRooms.TryGetValueAsync(tx, chatRoomId);
                if (chat.HasValue)
                {
                    IChatBroadcastService chatBroadcastService = GetChatBroadcastService();
                    chat.Value.AddChatter(username);
                    await chatBroadcastService.JoinChatRoom(connectionId, chatRoomId+region.ToString());
                    await tx.CommitAsync();
                }
                else
                {
                    await tx.CommitAsync();
                    return null;
                }
            }

            return chat.Value;
        }

        public async Task<bool> FavouriteChatRoom(string chatRoomId, string username)
        {
            IReliableDictionary<string, UserInfo> _userInfos = await StateManager.GetOrAddAsync<IReliableDictionary<string, UserInfo>>("userInfos");

            using (ITransaction tx = StateManager.CreateTransaction())
            {
                UserInfo userInfo = await _userInfos.GetOrAddAsync(tx, username, _ => new UserInfo());
                if (!userInfo.FavouritedChatsIds.Remove(chatRoomId))
                {
                    userInfo.FavouritedChatsIds.Add(chatRoomId);
                }
                await _userInfos.AddOrUpdateAsync(tx, username, userInfo, (_, __) => userInfo);
                await tx.CommitAsync();
            }

            return true;
        }

        public async Task<bool> LeaveChatRoom(string chatRoomId, string username, string connectionId, RegionsEnum region)
        {
            IReliableDictionary<string, Chat> _chatRooms = await StateManager.GetOrAddAsync<IReliableDictionary<string, Chat>>("chatRooms");

            using (ITransaction tx = StateManager.CreateTransaction())
            {
                ConditionalValue<Chat> chat = await _chatRooms.TryGetValueAsync(tx, chatRoomId);
                if (chat.HasValue)
                {
                    IChatBroadcastService chatBroadcastService = GetChatBroadcastService();
                    chat.Value.RemoveChatter(username);
                    await chatBroadcastService.LeaveChatRoom(connectionId, chatRoomId+region.ToString());
                    await tx.CommitAsync();
                }
                else
                {
                    await tx.CommitAsync();
                    return false;
                }
            }
            return true;
        }

        public async Task<bool> SendMessage(string chatRoomId, Message message, RegionsEnum region)
        {
            IReliableDictionary<string, Chat> _chatRooms = await StateManager.GetOrAddAsync<IReliableDictionary<string, Chat>>("chatRooms");
            IReliableDictionary<string, UserInfo> _userInfos = await StateManager.GetOrAddAsync<IReliableDictionary<string, UserInfo>>("userInfos");

            using (ITransaction tx = StateManager.CreateTransaction())
            {
                ConditionalValue<Chat> chat = await _chatRooms.TryGetValueAsync(tx, chatRoomId);
                if (chat.HasValue)
                {
                    IChatBroadcastService chatBroadcastService = GetChatBroadcastService();
                    chat.Value.AddMessage(message);
                    await chatBroadcastService.SendMessage(chatRoomId+region.ToString(), message);

                    UserInfo userInfo = await _userInfos.GetOrAddAsync(tx, message.Username, _ => new UserInfo());
                    userInfo.MessagesSent.Add(message);
                    await _userInfos.AddOrUpdateAsync(tx, message.Username, userInfo, (_, __) => userInfo);

                    await tx.CommitAsync();
                }
                else
                {
                    await tx.CommitAsync();
                    return false;
                }
            }
            return true;
        }

        public async Task<bool> DeleteChatRoom(string chatId)
        {
            IReliableDictionary<string, Chat> _chatRooms = await StateManager.GetOrAddAsync<IReliableDictionary<string, Chat>>("chatRooms");

            using (ITransaction tx = StateManager.CreateTransaction())
            {
                bool exists = await _chatRooms.ContainsKeyAsync(tx, chatId, LockMode.Update);
                if (exists)
                {
                    await _chatRooms.TryRemoveAsync(tx, chatId);
                    await tx.CommitAsync();
                    return true;
                }
                else
                {
                    await tx.CommitAsync();
                    return false;
                }
            }
        }

        private IChatBroadcastService GetChatBroadcastService( )
        {
            return _proxyFactory.CreateServiceProxy<IChatBroadcastService>(_chatBroadcastServiceUri);
        }

        /// <summary>
        /// Optional override to create listeners (like tcp, http) for this service instance.
        /// </summary>
        /// <returns>The collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return this.CreateServiceRemotingReplicaListeners();
        }
    }
}
