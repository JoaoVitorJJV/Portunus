using Portunus.Core.Crypto.DTO;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Portunus.Core.Crypto
{
    public static class VaultCipher
    {
        const int NonceSizeBytes = 12;
        const int TagSizeBytes = 16;

        public static EncryptionResult Encrypt(byte[] plaintext, byte[] key)
        {
            byte[] nonce = RandomNumberGenerator.GetBytes(NonceSizeBytes);

            byte[] cypherText = new byte[plaintext.Length];

            byte[] tag = new byte[TagSizeBytes];

            using AesGcm aes = new(key, TagSizeBytes);
            aes.Encrypt(nonce, plaintext, cypherText, tag);

            return new EncryptionResult(cypherText, nonce, tag);
        }

        public static bool TryDecrypt(byte[] ciphertext, byte[] nonce, byte[] tag, byte[] key, out byte[]? plaintext)
        {
            plaintext = new byte[ciphertext.Length];

            try
            {
                using AesGcm aes = new(key, TagSizeBytes);
                aes.Decrypt(nonce, ciphertext, tag, plaintext);
                return true;
            }
            catch (CryptographicException)
            {
                plaintext = null;
                return false;
            }
        }
    }
}
