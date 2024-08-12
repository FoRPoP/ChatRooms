using Interfaces;
using Interfaces.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Client;

namespace ChatRoomsWeb.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : Controller
    {

        private ServiceProxyFactory _proxyFactory;
        private Uri _serviceUri;

        public AuthController()
        {
            _proxyFactory = new ServiceProxyFactory(c => { return new FabricTransportServiceRemotingClientFactory(); });
            _serviceUri = new Uri("fabric:/Chat_Rooms/AuthService");
        }

        [HttpPost]
        [Route("Register")]
        public Task<bool> Register(string username, string password)
        {
            IAuthService authService = GetAuthService();
            return authService.Register(username, password);
        }

        [HttpPost]
        [Route("Login")]
        public Task<string> Login(string username, string password)
        {
            IAuthService authService = GetAuthService();
            return authService.Login(username, password);
        }

        private IAuthService GetAuthService()
        {
            return ServiceProxy.Create<IAuthService>(_serviceUri, new ServicePartitionKey(1));
        }

    }
}
