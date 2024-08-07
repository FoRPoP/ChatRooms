using Interfaces.Helpers;
using Microsoft.ServiceFabric.Services.Remoting;

namespace Interfaces
{
    public interface IAuthService : IService
    {
        Task<bool> Register(User user);
        Task<string> Login(User user);
    }
}
