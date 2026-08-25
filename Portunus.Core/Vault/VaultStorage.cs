namespace Portunus.Core.Vault
{
    public static class VaultStorage
    {
        public static void Save(byte[] data, string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);

            string temp = path + ".tmp";
            File.WriteAllBytes(temp, data);
            File.Move(temp, path, overwrite: true);
        }

        public static byte[] Load(string path) { return File.ReadAllBytes(path); }

        public static void DeleteVault(string path) {
            if (File.Exists(path)) File.Delete(path);
        }

        public static void ImportVault(string importPath, string targetPath)
        {
            File.Copy(importPath, targetPath, overwrite: true);
        }
    }

    public static class VaultLocation
    {
        private const string AppName = "Portunus";
        private const string AppFileName = "portunus.vault";
        private const string UserFileName = "user.json";

        public static string DefaultPath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            AppName,
            AppFileName
            );
    }
}
