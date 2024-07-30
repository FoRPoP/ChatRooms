using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Data;
using Interfaces;
using Interfaces.Helpers;
using Microsoft.ServiceFabric.Data.Collections;
using System.Fabric;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;

namespace ChatService
{
    /// <summary>
    /// The FabricRuntime creates an instance of this class for each service type instance.
    /// </summary>
    internal sealed class ChatService : StatefulService, IChatService
    {
        public ChatService(StatefulServiceContext context)
            : base(context)
        { }

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

        public async Task<bool> CreateChatRoom(string name, string description)
        {
            IReliableDictionary<string, Chat> _chatRooms = await StateManager.GetOrAddAsync<IReliableDictionary<string, Chat>>("chatRooms");
            IReliableDictionary<string, int> _IDs = await StateManager.GetOrAddAsync<IReliableDictionary<string, int>>("IDs");

            bool result;
            using (ITransaction tx = StateManager.CreateTransaction())
            {
                int nextChatRoomId = await _IDs.AddOrUpdateAsync(tx, "ChatRoomIDs", 0, (_, value) => value+1);
                ChatData chatData = new(nextChatRoomId.ToString(), name, description, "test");
                Chat chat = new(chatData);

                result = await _chatRooms.TryAddAsync(tx, chatData.ID, chat);
                await tx.CommitAsync();
            }

            return result;
        }

        public Task<Chat> JoinChatRoom(string chatRoomId, string userId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> LeaveChatRoom(string chatRoomId, string userId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SendMessage(string chatRoomId, string userId, Message message)
        {
            throw new NotImplementedException();
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
