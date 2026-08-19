using System;
using System.Collections.Generic;
using System.Text;

namespace Portunus.Platform.Enum
{
    public enum PresenceResult
    {
        Confirmed,
        Denied,      // cancelou ou falhou a verificação
        Unavailable  // sem Hello configurado nesta máquina
    }
}
