using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    [Table("refresh_tokens")]
    public class RefreshToken
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("user_id")]
        public required int UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public virtual User? User { get; set; }
        [Column("token")]
        public required string Token { get; set; }
        [Column("expires_at")]
        public required DateTime ExpiresAt { get; set; }
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }
        [NotMapped]
        public bool isExpired => DateTime.UtcNow >= ExpiresAt;
    }
}
