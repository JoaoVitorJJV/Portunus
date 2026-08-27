using Portunus.Platform.Interfaces;
using System;

#if MACOS
using Security;
using Foundation;
#endif

namespace Portunus.Platform.MacOS
{
    public class MacKeyStore : IKeyStore
    {
        public bool IsAvaliable => true;

        public bool Exists(string name)
        {
#if MACOS
            if (OperatingSystem.IsMacOS())
            {
                try
                {
                    var query = new SecRecord(SecKind.GenericPassword)
                    {
                        Service = "PortunusSafe",
                        Account = name
                    };

                    // Tenta buscar o registro sem retornar os dados (apenas checa existência)
                    var match = SecKeyChain.QueryAsRecord(query, out SecStatusCode err);
                    return err == SecStatusCode.Success && match != null;
                }
                catch { }
            }
#endif
            return false;
        }

        public bool TryKeyStore(string name, byte[] secret)
        {
#if MACOS
            if (OperatingSystem.IsMacOS())
            {
                try
                {
                    if (Exists(name))
                    {
                        Remove(name);
                    }

                    var record = new SecRecord(SecKind.GenericPassword)
                    {
                        Service = "PortunusSafe",
                        Account = name,
                        ValueData = NSData.FromArray(secret)
                    };

                    var err = SecKeyChain.Add(record);
                    return err == SecStatusCode.Success;
                }
                catch { }
            }
#endif
            return false;
        }

        public bool TryRetrieve(string name, out byte[] secret)
        {
            secret = [];
#if MACOS
            if (OperatingSystem.IsMacOS())
            {
                try
                {
                    var query = new SecRecord(SecKind.GenericPassword)
                    {
                        Service = "PortunusSafe",
                        Account = name
                    };

                    var match = SecKeyChain.QueryAsRecord(query, out SecStatusCode err);
                    
                    if (err == SecStatusCode.Success && match != null && match.ValueData != null)
                    {
                        secret = match.ValueData.ToArray();
                        return true;
                    }
                }
                catch { }
            }
#endif
            return false;
        }

        public void Remove(string name)
        {
#if MACOS
            if (OperatingSystem.IsMacOS())
            {
                var query = new SecRecord(SecKind.GenericPassword)
                {
                    Service = "PortunusSafe",
                    Account = name
                };
                SecKeyChain.Remove(query);
            }
#endif
        }
    }
}