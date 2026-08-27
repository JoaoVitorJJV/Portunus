using DesktopNotifications;
using DesktopNotifications.Apple;
using Portunus.Platform.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Portunus.Platform.MacOS
{
    public class MacNotificationNative : INativeNotificationService
    {
        public void ShowNative(string title, string body, string iconPath)
        {
            if (OperatingSystem.IsMacOS())
            {
                // Dispara em background para não travar a Thread
                Task.Run(async () =>
                {
                    try
                    {
                        using var manager = new AppleNotificationManager();
                        await manager.Initialize();
                        var notification = new Notification
                        {
                            Title = title,
                            Body = body
                        };

                        await manager.ShowNotification(notification);
                    }
                    catch (Exception)
                    {

                    }
                });
            }
        }
    }
}
