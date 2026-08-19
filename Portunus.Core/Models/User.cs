namespace Portunus.Core.Models
{
    public class User
    {
        public Guid Guid { get; set; } = Guid.NewGuid();
        public required string Name { get; set; }
        public required Machine Machine { get; set; }
        // Implementação futura
        public string? ProfilePic { get; set; }
    }

    public class Machine
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime DateJoined { get; set; }
    }
}
