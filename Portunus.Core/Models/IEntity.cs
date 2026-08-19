using System;
using System.Collections.Generic;
using System.Text;

namespace Portunus.Core.Models
{
    public interface IEntity
    {
        Guid Id { get; }
    }
}
