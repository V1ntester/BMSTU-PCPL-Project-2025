namespace API.Models.Responses
{
    public class TokenResponse
    {
        public required string AccessToken { get; set; }
        public required string RefreshToken { get; set; }
        public required DateTime ExpiresAt { get; set; }
    }
}
