using Portunus.Core.Crypto;
using Portunus.Core.Crypto.DTO;
using Portunus.Core.Models;
using Portunus.Core.Vault;
using System;
using System.Collections.Generic;
using System.Text;

namespace Portunus.Core.Tests.Core
{
    public class EnvelopeTests
    {
        const string CypherPassword = "Bananao123";
        const int TestMemory = 1024;
        const int TestIterations = 1;
        const int TestParallelism = 1;

        [Fact]
        public void Serialize_ThenDeserialize_ReturnsOriginalVault()
        {
            // ARRANGE
            var original = new VaultDocument
            {
                Passwords =
                {
                    new PasswordEntry { Name = "Gmail", Password = "senha-gmail-123" },
                    new PasswordEntry { Name = "GitHub", Password = "senha-github-456" }
                }
            };

            byte[] password = Encoding.UTF8.GetBytes(CypherPassword);
            byte[] salt = KeyDerivation.GenerateSalt();
            var parameters = new Argon2Params(TestMemory, TestIterations, TestParallelism);
            byte[] key = KeyDerivation.DeriveKey(password, salt, parameters.Memory, parameters.Iterations, parameters.Parallelism);

            // ACT
            byte[] envelopeBytes = Envelope.Serialize(original, key, salt, parameters);
            VaultEnvelope readBack = Envelope.ReadEnvelope(envelopeBytes);
            bool success = Envelope.TryDeserialize(readBack, key, out VaultDocument? result);

            // ASSERT
            Assert.True(success);
            Assert.NotNull(result);
            Assert.Equal(original.Passwords.Count, result.Passwords.Count);
            Assert.Equal(original.Passwords[0].Name, result.Passwords[0].Name);
            Assert.Equal(original.Passwords[0].Password, result.Passwords[0].Password);
            Assert.Equal(original.Passwords[1].Name, result.Passwords[1].Name);
        }
    }
}
