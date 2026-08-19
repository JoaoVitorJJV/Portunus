using Portunus.Platform.Windows;
using Portunus.Platform.MacOS;


namespace Portunus.Platform
{
    internal sealed class NullKeyStore : IKeyStore
    {
        public bool IsAvaliable => false;

        public bool TryKeyStore(string name, byte[] secret) => false;

        public bool TryRetrieve(string name, out byte[] secret)
        {
            secret = [];
            return false;
        }

        public void Remove(string name) { }
    }

    public static class PlatformKeyStore
    {
        public static IKeyStore Create(string directoryPath)
        {

            if (OperatingSystem.IsWindows())
                return new WindowsKeyStore(directoryPath);

            //if (OperatingSystem.IsMacOS())
            //    return new MacKeyStore();

            return new NullKeyStore();
        }
    }
}
