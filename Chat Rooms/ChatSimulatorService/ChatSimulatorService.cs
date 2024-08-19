using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Interfaces.Helpers;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace ChatSimulatorService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    public sealed class ChatSimulatorService : StatefulService
    {
        private readonly ChatServiceClient _chatServiceClient;
        private List<SimulatedUser> _simulatedUsers;

        public ChatSimulatorService(StatefulServiceContext context)
            : base(context)
        {
            _chatServiceClient = new();
            InitializeChatRooms();
            InitializeSimulatedUsers();
        }

        private void InitializeChatRooms()
        {
            for (int i = 0; i < 14; i++)
            {
                Random random = new();
                Array values = Enum.GetValues<RegionsEnum>();
                RegionsEnum region = (RegionsEnum)values.GetValue(random.Next(values.Length));

                _chatServiceClient.CreateChatRoomAsync("admin", $"Chat Room {i}", $"Description for Chat Room {i}", region);
            }
        }

        private void InitializeSimulatedUsers()
        {
            _simulatedUsers = new();
            for (int i = 0; i < 100; i++)
            {
                SimulatedUser user = new();
                _simulatedUsers.Add(user);
            }
        }

        /// <summary>
        /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        /// For more information on service communication, see https://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new ServiceReplicaListener[0];
        }

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var tasks = _simulatedUsers.Select(async user =>
            {
                user.RegisterAndLoginAsync();
                user.JoinChatRoomAsync();
                user.SimulateTypingAsync();
            });
            await Task.WhenAll(tasks);
        }
    }
}
