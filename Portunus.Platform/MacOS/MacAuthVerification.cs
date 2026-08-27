using Portunus.Platform.Interfaces;
using System;
using System.Threading.Tasks;

#if MACOS
using LocalAuthentication;
using Foundation;
#endif

namespace Portunus.Platform.MacOS
{
    public class MacAuthVerification : IAuthVerificationService
    {
        public async Task<bool> VerifyUserAsync(string promptMessage)
        {
#if MACOS
            if (OperatingSystem.IsMacOS())
            {
                try
                {
                    var context = new LAContext();
                    var authError = new NSError();
                    
                    // Verifica se o Mac possui Touch ID ou Senha configurados
                    if (context.CanEvaluatePolicy(LAPolicy.DeviceOwnerAuthentication, out authError))
                    {
                        // Exibe o popup nativo do Mac
                        var (success, error) = await context.EvaluatePolicyAsync(LAPolicy.DeviceOwnerAuthentication, promptMessage);
                        return success;
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