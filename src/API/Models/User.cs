using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    [Table("users")]
    public class User
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("name")]
        public required string Name { get; set; }
        [Column("surname")]
        public required string Surname { get; set; }
        [Column("email")]
        public required string Email { get; set; }
        [Column("password_hash")]
        public required string PasswordHash { get; set; }
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }
    };
}
