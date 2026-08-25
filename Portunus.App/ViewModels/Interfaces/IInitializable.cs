using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Portunus.App.ViewModels.Interfaces
{
    public interface IInitializable
    {
        Task InitializeAsync(object? parameter = null); 
    }
}
