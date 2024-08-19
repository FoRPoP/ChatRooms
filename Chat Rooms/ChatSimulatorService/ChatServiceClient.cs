using Interfaces;
using Interfaces.Helpers;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Client;

namespace ChatSimulatorService
{
    public class ChatServiceClient
    {
        private ServiceProxyFactory _proxyFactory;
        private readonly Uri _chatServiceUri;
        private readonly Uri _authServiceUri;

        public ChatServiceClient()
        {
            _proxyFactory = new ServiceProxyFactory(c => { return new FabricTransportServiceRemotingClientFactory(); });
            _chatServiceUri = new Uri("fabric:/Chat_Rooms/ChatService");
            _authServiceUri = new Uri("fabric:/Chat_Rooms/AuthService");
        }

        public async Task<string> RegisterAndLoginAsync(string username, string password)
        {
            IAuthService authService = GetAuthService();
            await authService.Register(username, password);
            return await authService.Login(username, password);
        }

        public async Task CreateChatRoomAsync(string username, string name, string description, RegionsEnum region)
        {
            IChatService chatService = GetChatService(region.ToString());
            await chatService.CreateChatRoom(username, name, description, region);
        }

        public async Task<Tuple<string, string>> JoinChatRoomAsync()
        {
            Random random = new();
            Array values = Enum.GetValues<RegionsEnum>();
            RegionsEnum region = (RegionsEnum)values.GetValue(random.Next(values.Length));

            IChatService chatService = GetChatService(region.ToString());

            Dictionary<string, ChatData> chatRooms = await chatService.GetChatRooms();
            List<string> chatRoomsIds = chatRooms.Keys.ToList();
            string chatRoomId = chatRoomsIds[random.Next(chatRoomsIds.Count)];

            return new Tuple<string, string>(chatRoomId, region.ToString());
        }

        public async Task SendMessageAsync(string chatRoomId, Message message, RegionsEnum region)
        {
            IChatService chatService = GetChatService(region.ToString());
            await chatService.SendMessage(chatRoomId, message, region);
        }


        private IAuthService GetAuthService()
        {
            return _proxyFactory.CreateServiceProxy<IAuthService>(_authServiceUri);
        }

        private IChatService GetChatService(string servicePartitionKeyName)
        {
            return ServiceProxy.Create<IChatService>(_chatServiceUri, new ServicePartitionKey(servicePartitionKeyName));
        }
    }
}
