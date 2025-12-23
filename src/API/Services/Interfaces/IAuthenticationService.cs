using API.Models;
using API.Models.Responses;

namespace API.Services
{
    public interface IAuthenticationService
    {
        Task<User> RegisterAsync(string name, string surname, string email, string password);
        Task<User?> AuthenticateAsync(string email, string password);
        Task<TokenResponse> GenerateTokensAsync(User user);
        Task<TokenResponse> RefreshTokenAsync(string accessToken, string refreshToken);
        Task RevokeRefreshTokenAsync(string refreshToken);
    }
}
