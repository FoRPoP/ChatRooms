using System.Fabric;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Data;
using Interfaces;
using Interfaces.Helpers;
using Microsoft.ServiceFabric.Data.Collections;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;

namespace AuthService
{
    /// <summary>
    /// The FabricRuntime creates an instance of this class for each service type instance.
    /// </summary>
    internal sealed class AuthService : StatefulService, IAuthService
    {
        private readonly IConfiguration _configuration;

        public AuthService(StatefulServiceContext context, IConfiguration configuration)
            : base(context)
        {
            _configuration = configuration;
        }

        public async Task<bool> Register(User newUser)
        {
            IReliableDictionary<string, User> _users = await StateManager.GetOrAddAsync<IReliableDictionary<string, User>>("users");

            using (ITransaction tx = StateManager.CreateTransaction())
            {
                ConditionalValue<User> user = await _users.TryGetValueAsync(tx, newUser.Username);
                if (user.HasValue)
                {
                    await tx.CommitAsync();
                    return false;
                }
                else
                {
                    newUser.HashedPassword = BCrypt.Net.BCrypt.HashPassword(newUser.HashedPassword);
                    await _users.AddAsync(tx, newUser.Username, newUser);
                    await tx.CommitAsync();
                    return true;
                }
            }
        }

        public async Task<string> Login(User user)
        {
            IReliableDictionary<string, User> _users = await StateManager.GetOrAddAsync<IReliableDictionary<string, User>>("users");

            using (ITransaction tx = StateManager.CreateTransaction())
            {
                ConditionalValue<User> userResult = await  _users.TryGetValueAsync(tx, user.Username);
                if (userResult.HasValue && BCrypt.Net.BCrypt.Verify(user.HashedPassword, userResult.Value.HashedPassword))
                {
                    return GenerateJwtToken(user.Username);
                }

                return "";
            }
        }

        private string GenerateJwtToken(string username)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: new[] {new Claim(ClaimTypes.Name, username)},
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
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
