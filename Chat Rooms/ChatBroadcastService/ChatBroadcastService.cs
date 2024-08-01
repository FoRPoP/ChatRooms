using System;
using System.Collections.Generic;
using System.Fabric;
using System.IO;
using System.Linq;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.AspNetCore.SignalR;
using Interfaces.Helpers;
using Interfaces;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.ServiceFabric.Services.Remoting.FabricTransport.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Runtime;

namespace ChatBroadcastService
{
    /// <summary>
    /// The FabricRuntime creates an instance of this class for each service type instance.
    /// </summary>
    internal sealed class ChatBroadcastService : StatelessService, IChatBroadcastService
    {

        private IHubContext<ChatBroadcastServiceHub> _chatBroadcastServiceHub;

        public ChatBroadcastService(StatelessServiceContext context)
            : base(context)
        { }

        public async Task JoinChatRoom(string connectionId, string chatRoomId)
        {
            await _chatBroadcastServiceHub.Groups.AddToGroupAsync(connectionId, chatRoomId);
        }

        public async Task LeaveChatRoom(string connectionId, string chatRoomId)
        {
            await _chatBroadcastServiceHub.Groups.RemoveFromGroupAsync(connectionId, chatRoomId);
        }

        public async Task SendMessage(string chatRoomId, Message message)
        {
            await _chatBroadcastServiceHub.Clients.Group(chatRoomId).SendAsync("ReceiveMessage", message);
        }

        /// <summary>
        /// Optional override to create listeners (like tcp, http) for this service instance.
        /// </summary>
        /// <returns>The collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[]
            {
                new ServiceInstanceListener((serviceContext) =>
                    {
                        return new FabricTransportServiceRemotingListener(serviceContext, this,
                            new FabricTransportRemotingListenerSettings() { EndpointResourceName = "ServiceEndpointV2" });
                    }, "RemotingListener"),
                new ServiceInstanceListener(serviceContext =>
                    new KestrelCommunicationListener(serviceContext, "ServiceEndpoint", (url, listener) =>
                    {
                        ServiceEventSource.Current.ServiceMessage(serviceContext, $"Starting Kestrel on {url}");
                        var webHost = new WebHostBuilder()
                                    .UseKestrel()
                                    .ConfigureAppConfiguration((builderContext, config) =>
                                    {
                                        config.Add(new JsonConfigurationSource { Path = "appsettings.json",  ReloadOnChange = true });
                                    })
                                    .ConfigureServices(
                                        services => services
                                            .AddSingleton<FabricClient>(new FabricClient())
                                            .AddSingleton<StatelessServiceContext>(serviceContext))
                                    .UseContentRoot(Directory.GetCurrentDirectory())
                                    .UseStartup<Startup>()
                                    .UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.None)
                                    .UseUrls(url)
                                    .Build();
                        _chatBroadcastServiceHub = webHost.Services.GetService<IHubContext<ChatBroadcastServiceHub>>();
                        return webHost;
                    }), "KestrelListener")
            };
        }
    }
}
