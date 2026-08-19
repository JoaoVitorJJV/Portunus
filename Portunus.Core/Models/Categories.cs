namespace Portunus.Core.Models
{
    public class Category : IEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string Name { get; set; }
        public string? Description { get; set; }
        public string? BadgeColor { get; set; }
    }
}
