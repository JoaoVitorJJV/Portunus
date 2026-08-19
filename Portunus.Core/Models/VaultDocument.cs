namespace Portunus.Core.Models
{
    public class VaultDocument
    {
        public int SchemaVersion { get; set; } = 1;
        public List<Vault> Vaults { get; set; } = [];
        public List<PasswordEntry> Passwords { get; set; } = [];
        public List<PasswordTag> Tags { get; set; } = [];
        public List<Category> Categories { get; set; } = [];
    }
}
