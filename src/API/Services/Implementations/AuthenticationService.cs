using API.Data;
using API.Models;
using API.Models.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using BCrypt.Net;
using System.Text;

namespace API.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly AppDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly IConfiguration _configuration;

        public AuthenticationService(AppDbContext context, IJwtService generationService, IConfiguration configuration)
        {
            _context = context;
            _jwtService = generationService;
            _configuration = configuration;
        }

        public async Task<User> RegisterAsync(string name, string surname, string email, string password)
        {
            if (await _context.Users.AnyAsync(u => u.Email == email)) throw new Exception("Email already exists");

            var user = new User
            {
                Name = name,
                Surname = surname,
                Email = email,
                PasswordHash = HashPassword(password),
                CreatedAt = DateTime.UtcNow
            };

            _context.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<User> AuthenticateAsync(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null || !VerifyPassword(password, user.PasswordHash)) return null;

            return user;
        }

        public async Task<TokenResponse> GenerateTokensAsync(User user)
        {
            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();

            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(_configuration.GetValue<int>("JwtSettings:RefreshTokenExpirationDays")),
                CreatedAt = DateTime.UtcNow
            };

            _context.RefreshTokens.Add(refreshTokenEntity);
            await _context.SaveChangesAsync();

            return new TokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_configuration.GetValue<int>("JwtSettings::AccessTokenExpirationMinutes"))
            };
        }

        public async Task<TokenResponse> RefreshTokenAsync(string accessToken, string refreshToken)
        {
            var principal = _jwtService.GetPrincipalFromExpiredToken(accessToken);
            var userId = int.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new SecurityTokenException("Invalid token"));

            var storedRefreshToken = await _context.RefreshTokens.Include(rt => rt.User).FirstOrDefaultAsync(rt => rt.Token == refreshToken && rt.UserId == userId);

            if (storedRefreshToken == null || storedRefreshToken.isExpired) throw new SecurityTokenException("Invalid refresh token");

            var newAccessToken = _jwtService.GenerateAccessToken(storedRefreshToken.User);
            var newRefreshToken = _jwtService.GenerateRefreshToken();

            _context.Remove(storedRefreshToken);

            var newRefreshTokenEntity = new RefreshToken
            {
                UserId = storedRefreshToken.UserId,
                User = storedRefreshToken.User,
                Token = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(_configuration.GetValue<int>("JwtSettings:RefreshTokenExpirationDays")),
                CreatedAt = DateTime.UtcNow
            };

            _context.Add(newRefreshTokenEntity);
            await _context.SaveChangesAsync();

            return new TokenResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_configuration.GetValue<int>("JwtSettings:AccessTokenExpirationMinutes"))
            };
        }

        public async Task RevokeRefreshTokenAsync(string refreshToken)
        {
            var token = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            if (token != null)
            {
                _context.RefreshTokens.Remove(token);
                await _context.SaveChangesAsync();
            }
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.EnhancedHashPassword(password, HashType.SHA256);
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            try
            {
                return BCrypt.Net.BCrypt.EnhancedVerify(password, storedHash, HashType.SHA256);
            }
            catch (SaltParseException)
            {
                return false;
            }
        }
    }
}