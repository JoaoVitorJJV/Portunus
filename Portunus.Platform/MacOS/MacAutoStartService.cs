using Portunus.Platform.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Portunus.Platform.MacOS
{
    public class MacAutoStartService : IAutoStartService
    {
        private const string PlistFileName = "com.portunus.app.plist";

        private string PlistFilePath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "Library", "LaunchAgents", PlistFileName);

        public void EnableAutoStart()
        {
            if (OperatingSystem.IsMacOS())
            {
                var executablePath = Process.GetCurrentProcess().MainModule?.FileName;
                if (executablePath == null) return;

                var plistContent = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
                    <!DOCTYPE plist PUBLIC ""-//Apple//DTD PLIST 1.0//EN"" ""http://www.apple.com/DTDs/PropertyList-1.0.dtd"">
                    <plist version=""1.0"">
                    <dict>
                        <key>Label</key>
                        <string>com.portunus.app</string>
                        <key>ProgramArguments</key>
                        <array>
                            <string>{executablePath}</string>
                            <string>--hidden</string>
                        </array>
                        <key>RunAtLoad</key>
                        <true/>
                    </dict>
                    </plist>";

                // Garante que o diretório existe
                var directory = Path.GetDirectoryName(PlistFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory!);
                }

                File.WriteAllText(PlistFilePath, plistContent);
            }
        }

        public void DisableAutoStart()
        {
            if (OperatingSystem.IsMacOS())
            {
                if (File.Exists(PlistFilePath))
                {
                    File.Delete(PlistFilePath);
                }
            }
        }

        public bool IsAutoStartEnabled()
        {
            if (OperatingSystem.IsMacOS())
            {
                return File.Exists(PlistFilePath);
            }
            return false;
        }
    }
}
