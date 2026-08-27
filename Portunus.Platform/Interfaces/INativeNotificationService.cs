using System;
using System.Collections.Generic;
using System.Text;

namespace Portunus.Platform.Interfaces
{
    public interface INativeNotificationService
    {
        public void ShowNative(string title, string body, string iconPath);
    }
}
