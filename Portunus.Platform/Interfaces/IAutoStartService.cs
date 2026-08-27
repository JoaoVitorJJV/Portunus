using System;
using System.Collections.Generic;
using System.Text;

namespace Portunus.Platform.Interfaces
{
    public interface IAutoStartService
    {
        void EnableAutoStart();
        void DisableAutoStart();
        bool IsAutoStartEnabled();
    }
}
