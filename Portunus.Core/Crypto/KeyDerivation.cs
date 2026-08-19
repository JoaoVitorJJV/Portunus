using Konscious.Security.Cryptography;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Portunus.Core.Crypto
{
    public static class KeyDerivation
    {
        const int BYTES_PASSWORD_MAIN = 32;

        public static byte[] DeriveKey(byte[] password, byte[] salt, int memory, int iterations, int paralellism)
        {
            Argon2id motor = new (password)
            {
                Salt = salt,
                MemorySize = memory,
                Iterations = iterations,
                DegreeOfParallelism = paralellism
            };

            return motor.GetBytes(BYTES_PASSWORD_MAIN);
        }

        public static byte[] GenerateSalt(int size = 16)
        {
            return RandomNumberGenerator.GetBytes(size);
        }

    }
}
