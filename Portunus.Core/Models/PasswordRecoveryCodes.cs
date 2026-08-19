
namespace Portunus.Core.Models
{
    public class PasswordRecoveryCode
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string Name { get; set; }
        public List<RecoveryCodeEntry> Codes { get; set; } = [];
        public string? Description { get; set; }
    }

    public class RecoveryCodeEntry
    {
        public required string Code { get; set; }
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        public DateTime? DateUsed { get; set; }
        public DateTime? DateExpiration { get; set; }
    }
}
