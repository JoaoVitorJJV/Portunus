using Portunus.Core.Crypto;
using Portunus.Core.Crypto.DTO;
using Portunus.Core.Extensions;
using Portunus.Core.Models;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Portunus.Core.Vault
{
    public sealed class VaultSession : IDisposable
    {
        private readonly byte[] _key;
        private readonly byte[] _salt;
        private readonly Argon2Params _parameters;
        private VaultDocument _data;
        private bool _disposed;
        private readonly string _path;

        #region Vault Data
        public IReadOnlyList<PasswordEntry> Entries => _data.Passwords;
        public IReadOnlyList<PasswordTag> Tags => _data.Tags;
        public IReadOnlyList<Category> Categories => _data.Categories;
        public IReadOnlyList<Models.Vault> Vaults => _data.Vaults;
        #endregion


        private VaultSession(byte[] key, byte[] salt, Argon2Params parameters, VaultDocument data, string path)
        {
            _key = key; _salt = salt; _parameters = parameters; _data = data; _path = path;
        }

        public static VaultSession CreateNew(string masterPassword, string? path)
        {
            byte[] passwordBytes = Encoding.UTF8.GetBytes(masterPassword);
            byte[] salt = KeyDerivation.GenerateSalt();
            Argon2Params parameters = Argon2Params.Default;
            string pathLocation = path ?? VaultLocation.DefaultPath;

            byte[] key = KeyDerivation.DeriveKey(
                passwordBytes, salt, parameters.Memory, parameters.Iterations, parameters.Parallelism);
            CryptographicOperations.ZeroMemory(passwordBytes);   

            try
            {
                var data = new VaultDocument();
                data.Vaults.Add(new Models.Vault { 
                    Name = "Pessoal", 
                    DateCreated = DateTime.UtcNow, 
                    DateUpdated = DateTime.UtcNow 
                });

                byte[] envelopeBytes = Envelope.Serialize(data, key, salt, parameters);
                VaultStorage.Save(envelopeBytes, pathLocation);

                return new VaultSession(key, salt, parameters, data, pathLocation);  
            }
            catch
            {
                CryptographicOperations.ZeroMemory(key); 
                throw;
            }
        }


        /// <summary>
        /// Tenta destravar um cofre existente. Retorna <c>false</c> se a senha estiver
        /// incorreta (ou o arquivo estiver adulterado). Lança <see cref="FileNotFoundException"/>
        /// se não houver cofre no caminho informado — o chamador deve verificar a existência
        /// antes, ou tratar a exceção para orientar o usuário a criar um cofre.
        /// </summary>
        /// <exception cref="FileNotFoundException">Nenhum cofre existe em <paramref name="path"/>.</exception>
        public static bool TryUnlock(string? path, string masterPassword, out VaultSession? session)
        {
            session = null;
            string pathLocation = path ?? VaultLocation.DefaultPath;

            if (!File.Exists(pathLocation))
                throw new FileNotFoundException("Nenhum cofre encontrado no caminho informado.", pathLocation);

            VaultEnvelope envelope = Envelope.ReadEnvelope(VaultStorage.Load(pathLocation));

            byte[] passwordBytes = Encoding.UTF8.GetBytes(masterPassword);
            byte[] key;
            try
            {
                key = KeyDerivation.DeriveKey(
                    passwordBytes, envelope.Salt,
                    envelope.Parameters.Memory, envelope.Parameters.Iterations, envelope.Parameters.Parallelism);
            }
            finally
            {
                CryptographicOperations.ZeroMemory(passwordBytes);
            }

            if (!Envelope.TryDeserialize(envelope, key, out VaultDocument? data))
            {
                CryptographicOperations.ZeroMemory(key);
                return false;
            }

            session = new VaultSession(key, envelope.Salt, envelope.Parameters, data!, pathLocation);
            return true;
        }


        #region CRUD - Cofre
        private void Mutate(Action<VaultDocument> change)
        {
            EnsureUnlocked();
            change(_data);
            Save();
        }

        public void Save()
        {
            EnsureUnlocked();
            byte[] envelopeBytes = Envelope.Serialize(_data, _key, _salt, _parameters);
            VaultStorage.Save(envelopeBytes, _path);
        }

        #region Upsert
        public void SaveEntry(PasswordEntry e) => Mutate(d =>
        {
            e.DateUpdated = DateTime.UtcNow;
            d.Passwords.Upsert(e);

        });
        public void SaveTag(PasswordTag t) => Mutate(d => d.Tags.Upsert(t));
        public void SaveCategory(Category c) => Mutate(d => d.Categories.Upsert(c));
        public void SaveVault(Models.Vault v) => Mutate(d =>
        {
            v.DateUpdated = DateTime.UtcNow;
            d.Vaults.Upsert(v);
        });

        #endregion

        #region Delete
        public void DeleteEntry(Guid id) => Mutate(d => d.Passwords.RemoveById(id));

        public void DeleteTag(Guid id) => Mutate(d =>
        {
            d.Tags.RemoveById(id);
            foreach (var p in d.Passwords)
                p.TagIds.Remove(id);
        });

        public void DeleteCategory(Guid id) => Mutate(d =>
        {
            d.Categories.RemoveById(id);
            foreach (var p in d.Passwords.Where(p => p.CategoryId == id))
                p.CategoryId = null;
        });

        public bool DeleteVault(Guid id)
        {
            EnsureUnlocked();
            if (_data.Vaults.Count <= 1) return false;

            Mutate(d =>
            {
                d.Passwords.RemoveAll(p => p.VaultId == id);
                d.Vaults.RemoveById(id);
            });
            return true;
        }
        #endregion

        #endregion


        private void EnsureUnlocked()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(VaultSession));
        }

        public void Dispose()
        {
            if (_disposed) return;
            CryptographicOperations.ZeroMemory(_key);
            _data = null!;
            _disposed = true;
        }
    }
}
