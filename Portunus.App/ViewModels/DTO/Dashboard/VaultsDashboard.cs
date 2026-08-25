using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Portunus.App.ViewModels.DTO.Dashboard
{
    public partial class VaultsDashboard : ObservableObject
    {
        public required Guid Id {  get; set; }
        public string Icon { get; set; } = "fa-solid fa-house";
        public required string VaultName {  get; set; }

        [ObservableProperty]
        private int _count;

        [ObservableProperty]
        private bool _isSelected;
    }
}
