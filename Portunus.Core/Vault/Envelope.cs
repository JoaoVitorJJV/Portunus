using Portunus.Core.Crypto;
using Portunus.Core.Crypto.DTO;
using Portunus.Core.Models;
using System.Text.Json;

namespace Portunus.Core.Vault
{
    /// <summary>
    /// Translates between <see cref="VaultDocument"/> objects and the binary <c>.vault</c> file format.
    /// Handles JSON serialization, encryption, and the binary envelope layout. Does not touch disk —
    /// it produces and consumes byte arrays; reading and writing files is a separate concern.
    /// </summary>
    public static class Envelope
    {
        private const byte FormatVersion = 1;
        private const int SaltSize = 16;
        private const int NonceSize = 12;
        private const int TagSize = 16;

        /// <summary>
        /// Packs a vault into its final binary envelope. Serializes the <see cref="VaultDocument"/>
        /// to JSON, encrypts it with the provided key, and assembles the complete
        /// <c>.vault</c> byte layout: [version][salt][params][nonce][tag][ciphertext].
        /// The salt and parameters are not used for encryption here — they are written into
        /// the envelope so the key can be re-derived when the vault is later opened.
        /// </summary>
        /// <param name="data">The vault contents (passwords, tags, schema version) to pack.</param>
        /// <param name="key">The 32-byte encryption key, already derived from the master password.</param>
        /// <param name="salt">The salt used to derive <paramref name="key"/>. Stored in the envelope for re-derivation.</param>
        /// <param name="parameters">The Argon2id parameters used to derive <paramref name="key"/>. Stored in the envelope for re-derivation.</param>
        /// <returns>The complete encrypted envelope as a byte array, ready to be written to disk.</returns>
        public static byte[] Serialize(VaultDocument data, byte[] key, byte[] salt, Argon2Params parameters)
        {
            // Serialize the VaultData object to a byte array
            byte[] plaintext = JsonSerializer.SerializeToUtf8Bytes(data);

            // Encrypt the serialized data using the provided key
            EncryptionResult enc = VaultCipher.Encrypt(plaintext, key);

            // Return a byte layout: [version][salt][params][nonce][tag][ciphertext].
            using var stream = new MemoryStream();
            stream.WriteByte(FormatVersion);
            stream.Write(salt);
            stream.Write(BitConverter.GetBytes(parameters.Memory));
            stream.Write(BitConverter.GetBytes(parameters.Iterations));
            stream.Write(BitConverter.GetBytes(parameters.Parallelism));
            stream.Write(enc.Nonce);
            stream.Write(enc.Tag);
            stream.Write(enc.Ciphertext);

            return stream.ToArray();
        }

        public static bool TryDeserialize(VaultEnvelope envelope, byte[] key, out VaultDocument? data)
        {
            data = null;

            if (!VaultCipher.TryDecrypt(envelope.Ciphertext, envelope.Nonce, envelope.Tag, key, out byte[]? plaintext))
                return false;   // wrong password or tampered ciphertext

            data = JsonSerializer.Deserialize<VaultDocument>(plaintext);
            return true;
        }

        public static VaultEnvelope ReadEnvelope(byte[] envelope)
        {
            int position = 0;

            // 1 byte: versão do formato
            byte version = envelope[position];
            position += 1;

            // 16 bytes: salt
            byte[] salt = new byte[SaltSize];
            Array.Copy(envelope, position, salt, 0, SaltSize);
            position += SaltSize;

            // Ajustar depois para o cross-platform
            // 4 bytes cada: os três params Argon2id
            int memory = BitConverter.ToInt32(envelope, position);
            position += 4;
            int iterations = BitConverter.ToInt32(envelope, position);
            position += 4;
            int parallelism = BitConverter.ToInt32(envelope, position);
            position += 4;

            // 12 bytes: nonce
            byte[] nonce = new byte[NonceSize];
            Array.Copy(envelope, position, nonce, 0, NonceSize);
            position += NonceSize;

            // 16 bytes: tag GCM
            byte[] tag = new byte[TagSize];
            Array.Copy(envelope, position, tag, 0, TagSize);
            position += TagSize;

            // resto: ciphertext (tamanho variável)
            byte[] ciphertext = new byte[envelope.Length - position];
            Array.Copy(envelope, position, ciphertext, 0, ciphertext.Length);

            return new VaultEnvelope(
                version,
                salt,
                new Argon2Params(memory, iterations, parallelism),
                nonce,
                tag,
                ciphertext);
        }
    }

}

