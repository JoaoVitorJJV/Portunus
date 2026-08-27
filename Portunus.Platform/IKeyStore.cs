
namespace Portunus.Platform
{
    public interface IKeyStore
    {
        bool IsAvaliable {  get; }
        bool TryKeyStore(string name, byte[] secret);
        bool TryRetrieve(string name, out byte[] secret);
        void Remove(string name);
        bool Exists(string name);

    }
}
