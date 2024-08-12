using Interfaces.Helpers;
using Microsoft.ServiceFabric.Services.Remoting;

namespace Interfaces
{
    public interface IAuthService : IService
    {
        Task<bool> Register(string username, string password);
        Task<string> Login(string username, string password);
    }
}
