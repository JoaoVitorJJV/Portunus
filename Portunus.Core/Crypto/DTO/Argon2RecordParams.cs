using System;
using System.Collections.Generic;
using System.Text;

namespace Portunus.Core.Crypto.DTO
{
    public record Argon2Params(int Memory, int Iterations, int Parallelism)
    {
        public static Argon2Params Default => new(65536, 3, 1);
    }
}
