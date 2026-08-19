
namespace Portunus.Core.Crypto.DTO
{
    public record EncryptionResult(byte[] Ciphertext, byte[] Nonce, byte[] Tag);

    public record VaultEnvelope(
    byte Version,
    byte[] Salt,
    Argon2Params Parameters,
    byte[] Nonce,
    byte[] Tag,
    byte[] Ciphertext);
}
