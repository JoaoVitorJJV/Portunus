using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Portunus.App.Util
{
    public static class AppUtils
    {
        public static string ResolveVersion() =>
    Assembly.GetExecutingAssembly()
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
        ?.InformationalVersion.Split('+')[0]
        ?? "dev";
    }
}
