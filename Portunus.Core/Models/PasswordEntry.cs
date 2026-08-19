
namespace Portunus.Core.Models
{
    public class PasswordEntry : IEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid VaultId { get; set; }
        public Guid? CategoryId { get; set; }
        public List<Guid> TagIds { get; set; } = [];
        public List<RecoveryCodeEntry> RecoveryCodes { get; set; } = [];
        public List<PasswordNote> Notes { get; set; } = [];
        public required string Password { get; set; }
        public string? Username { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public DateTime? DateToChangePass { get; set; }
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        public DateTime? DateUpdated { get; set; }
        public DateTime? DateToFavorited { get; set; }
        public string? Url { get; set; }
    }

    public class PasswordTag : IEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string Name { get; set; }
        public string? BadgeColor { get; set; }
        public string? Description { get; set; }
    }

    public class PasswordNote
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string Content { get; set; } 
        public string? Name { get; set; }            
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        public DateTime? DateUpdated { get; set; }
    }
}
