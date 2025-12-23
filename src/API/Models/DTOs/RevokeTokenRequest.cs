namespace API.Models.Requests
{
    public class RevokeTokenRequest
    {
        public required string RefreshToken { get; set; }
    }
}
