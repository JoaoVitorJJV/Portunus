namespace Portunus.Core.DTO
{
    public class CreateVaultDTO
    {
        public required string MasterPassword { get; set; }
        public required string VaultName {  get; set; }
        public string? Path {  get; set; }
    }
}
