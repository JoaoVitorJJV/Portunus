using Microsoft.Win32;
using Portunus.Platform.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Portunus.Platform.Windows
{
    public class WindowsAutoStartService : IAutoStartService
    {
        private const string RegistryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string AppKeyName = "Portunus";

        public void EnableAutoStart()
        {
            if (OperatingSystem.IsWindows())
            {
                using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true);
                var executablePath = Process.GetCurrentProcess().MainModule?.FileName;

                if (key != null && executablePath != null)
                {
                    key.SetValue(AppKeyName, $"\"{executablePath}\" --hidden");
                }
            }
        }

        public void DisableAutoStart()
        {
            if (OperatingSystem.IsWindows())
            {
                using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true);
                if (key != null)
                {
                    key.DeleteValue(AppKeyName, false);
                }
            }
        }

        public bool IsAutoStartEnabled()
        {
            if (OperatingSystem.IsWindows())
            {
                using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, false);
                return key?.GetValue(AppKeyName) != null;
            }
            return false;
        }
    }
}
