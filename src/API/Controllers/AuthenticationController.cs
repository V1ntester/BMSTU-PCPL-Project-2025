using API.Services;
using API.Models.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationService _authenticationService;

        public AuthenticationController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                var user = await _authenticationService.RegisterAsync(request.Name, request.Surname, request.Email, request.Password);
                var tokens = await _authenticationService.GenerateTokensAsync(user);
                return Ok(tokens);
            }
            catch (Exception exception)
            {
                return BadRequest(new { message = exception.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _authenticationService.AuthenticateAsync(request.Email, request.Password);

            if (user is null) return Unauthorized(new { message = "Invalid credentials" });

            var tokens = await _authenticationService.GenerateTokensAsync(user);
            return Ok(tokens);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
        {
            try
            {
                var tokens = await _authenticationService.RefreshTokenAsync(request.AccessToken, request.RefreshToken);
                return Ok(tokens);
            }
            catch (SecurityTokenException exception)
            {
                return Unauthorized(new { message = exception.Message });
            }
        }

        [HttpPost("revoke")]
        [Authorize]
        public async Task<IActionResult> Revoke([FromBody] RevokeTokenRequest request)
        {
            await _authenticationService.RevokeRefreshTokenAsync(request.RefreshToken);
            return Ok(new { message = "Token revoked" });
        }
    }
}
