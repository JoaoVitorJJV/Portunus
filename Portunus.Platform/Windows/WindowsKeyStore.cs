using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using System.Security.Cryptography;

namespace Portunus.Platform.Windows
{
    [SupportedOSPlatform("windows")]
    internal sealed class WindowsKeyStore(string storageDirectory) : IKeyStore
    {
        public bool IsAvaliable => true;
        public readonly string _storageDirectory = storageDirectory;

        public bool TryKeyStore(string name, byte[] secret)
        {
            try
            {

                Directory.CreateDirectory(_storageDirectory);

                byte[] encrypted = ProtectedData.Protect(
                    secret,
                    optionalEntropy: null,
                    scope: DataProtectionScope.CurrentUser
                );

                File.WriteAllBytes(_storageDirectory, encrypted);
                return true;
            }
            catch(Exception)
            {
                return false;
            }

        }
        public bool TryRetrieve(string name, out byte[] secret)
        {
            secret = [];

            try
            {
                string path = PathFor(name);
                if (!File.Exists(path))
                    return false;

                byte[] encrypted = File.ReadAllBytes(path);

                secret = ProtectedData.Unprotect(
                    encrypted,
                    optionalEntropy: null,
                    DataProtectionScope.CurrentUser);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void Remove(string name)
        {
            string path = PathFor(name);
            if (File.Exists(path))
                File.Delete(path);
        }

        private string PathFor(string name) =>
            Path.Combine(_storageDirectory, name + ".bin");
    }
}
