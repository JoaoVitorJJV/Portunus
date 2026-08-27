using Portunus.Platform.Interfaces;
using System;
using System.Threading.Tasks;

#if WINDOWS
using Windows.Security.Credentials.UI;
#endif

namespace Portunus.Platform.Windows
{
    public class WindowsAuthVerification : IAuthVerificationService
    {
        public async Task<bool> VerifyUserAsync(string promptMessage)
        {
#if WINDOWS
            if (OperatingSystem.IsWindows())
            {
                try
                {
                    var availability = await UserConsentVerifier.CheckAvailabilityAsync();

                    if (availability == UserConsentVerifierAvailability.Available)
                    {
                        var result = await UserConsentVerifier.RequestVerificationAsync(promptMessage);
                        return result == UserConsentVerificationResult.Verified;
                    }
                }
                catch (Exception)
                {
                    
                }
            }
#endif
            return false;
        }
    }
}