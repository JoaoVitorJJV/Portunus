using System;
using System.Collections.Generic;
using System.Text;

namespace Portunus.App.Domain.Util
{
    public static class Utilities
    {
        public static byte[] GetBytesFromString(string str)
        {
            return Encoding.UTF8.GetBytes(str);
        }
    }
}
