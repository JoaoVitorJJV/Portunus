using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace Portunus.App.ViewModels.DTO.Dashboard
{
    public class Categories
    {
        public required Guid Id { get; set; }
        public required string Name { get; set; }
    }

    public partial class CategoriesDashboard : ObservableObject
    {
        public required Guid Id { get; set; }
        public required string Name { get; set; }
        public string Badge { get; set; } = string.Empty;

        [ObservableProperty]
        private int _count;
    }
}
