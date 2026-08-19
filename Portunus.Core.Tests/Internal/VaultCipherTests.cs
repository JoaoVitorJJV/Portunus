using Portunus.Core.Crypto;
using Portunus.Core.Crypto.DTO;
using System.Text;

namespace Portunus.Core.Tests.Core
{
    public class VaultCipherTests
    {
        const int TestMemory = 1024;      // KB
        const int TestIterations = 1;
        const int TestParallelism = 1;
        const string CypherText = "senha de teste do gmail";
        const string CypherPassword = "Bananao123";

        [Fact]
        public void Encrypt_ThenDecrypt_ReturnsOriginalPlaintext()
        {
            // ARRANGE
            byte[] plaintext = Encoding.UTF8.GetBytes(CypherText);
            byte[] password = Encoding.UTF8.GetBytes(CypherPassword);
            byte[] salt = KeyDerivation.GenerateSalt();
            byte[] key = KeyDerivation.DeriveKey(password, salt, TestMemory, TestIterations, TestParallelism);

            // ACT
            EncryptionResult result = VaultCipher.Encrypt(plaintext, key);
            bool success = VaultCipher.TryDecrypt(
                result.Ciphertext, result.Nonce, result.Tag, key, out byte[]? decrypted);

            // ASSERT
            Assert.True(success);
            Assert.Equal(plaintext, decrypted);
        }

        [Fact]
        public void DecryptWithWrongPassWord_ReturnsFalse()
        {
            // ARRANGE
            byte[] plaintext = Encoding.UTF8.GetBytes(CypherText);
            byte[] password = Encoding.UTF8.GetBytes(CypherPassword);
            byte[] wrongPassword = Encoding.UTF8.GetBytes("WrongPassword");
            byte[] salt = KeyDerivation.GenerateSalt();
            byte[] key = KeyDerivation.DeriveKey(password, salt, TestMemory, TestIterations, TestParallelism);
            byte[] wrongKey = KeyDerivation.DeriveKey(wrongPassword, salt, TestMemory, TestIterations, TestParallelism);

            // ACT
            EncryptionResult result = VaultCipher.Encrypt(plaintext, key);
            bool success = VaultCipher.TryDecrypt(
                result.Ciphertext, result.Nonce, result.Tag, wrongKey, out byte[]? decrypted);

            // ASSERT
            Assert.False(success);
            Assert.Null(decrypted);
        }

        [Fact]
        public void TemperedCipherText_ReturnsFalse()
        {
            // ARRANGE
            byte[] plaintext = Encoding.UTF8.GetBytes(CypherText);
            byte[] password = Encoding.UTF8.GetBytes(CypherPassword);
            byte[] salt = KeyDerivation.GenerateSalt();
            byte[] key = KeyDerivation.DeriveKey(password, salt, TestMemory, TestIterations, TestParallelism);

            // ACT
            EncryptionResult result = VaultCipher.Encrypt(plaintext, key);
            // Manually tamper with the ciphertext
            result.Ciphertext[0] ^= 0xFF;
            bool success = VaultCipher.TryDecrypt(
                result.Ciphertext, result.Nonce, result.Tag, key, out byte[]? decrypted);

            // ASSERT
            Assert.False(success);
            Assert.Null(decrypted);
        }

        [Fact]
        public void SameInput_ProduceSameKey()
        {
            // ARRANGE
            byte[] password = Encoding.UTF8.GetBytes(CypherPassword);
            byte[] salt = KeyDerivation.GenerateSalt();
            // ACT
            byte[] key1 = KeyDerivation.DeriveKey(password, salt, TestMemory, TestIterations, TestParallelism);
            byte[] key2 = KeyDerivation.DeriveKey(password, salt, TestMemory, TestIterations, TestParallelism);
            // ASSERT
            Assert.Equal(key1, key2);
        }

        [Fact]
        public void DifferentSalts_ProducesDifferentKeys()
        {
            // ARRANGE
            byte[] password = Encoding.UTF8.GetBytes(CypherPassword);
            byte[] salt1 = KeyDerivation.GenerateSalt();
            byte[] salt2 = KeyDerivation.GenerateSalt();
            // ACT
            byte[] key1 = KeyDerivation.DeriveKey(password, salt1, TestMemory, TestIterations, TestParallelism);
            byte[] key2 = KeyDerivation.DeriveKey(password, salt2, TestMemory, TestIterations, TestParallelism);
            // ASSERT
            Assert.NotEqual(key1, key2);
        }
    }
}
