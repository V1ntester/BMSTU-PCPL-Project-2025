using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    [Table("refresh_tokens")]
    public class RefreshToken
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("user_id")]
        public int UserId { get; set; }
        public User User { get; set; }
        [Column("token")]
        public string Token { get; set; }
        [Column("expires_at")]
        public DateTime ExpiresAt { get; set; }
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }
        public bool isExpired => DateTime.UtcNow >= ExpiresAt;
    }
}