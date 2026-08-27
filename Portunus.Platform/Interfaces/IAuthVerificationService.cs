using System;
using System.Collections.Generic;
using System.Text;

namespace Portunus.Platform.Interfaces
{
    public interface IAuthVerificationService
    {
        Task<bool> VerifyUserAsync(string promptMessage);
    }
}
