namespace Portunus.Core.Models
{
    public class Vault : IEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string Name { get; set; }
        public string? Description { get; set; }
        public string? BadgeColor { get; set; }
        public string? Icon { get; set; }
        public int CountEntry { get; set; } = 0;
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        public DateTime? DateUpdated { get; set; }
    }
}
