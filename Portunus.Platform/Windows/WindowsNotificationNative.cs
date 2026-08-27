using DesktopNotifications;
using Portunus.Platform.Interfaces;
using System;
using System.Threading.Tasks;

#if WINDOWS
using DesktopNotifications.Windows;
#endif

namespace Portunus.Platform.Windows
{
    public class WindowsNotificationNative : INativeNotificationService
    {
        public void ShowNative(string title, string body, string iconPath)
        {
#if WINDOWS
            if (OperatingSystem.IsWindows())
            {
                Task.Run(async () =>
                {
                    try
                    {
                        var context = WindowsApplicationContext.FromCurrentProcess("Portunus");
                        
                        using var manager = new WindowsNotificationManager(context);
                        await manager.Initialize();

                        var notification = new Notification
                        {
                            Title = title,
                            Body = body
                        };

                        await manager.ShowNotification(notification, null);
                    }
                    catch (Exception)
                    {
                    }
                });
            }
#endif
        }
    }
}