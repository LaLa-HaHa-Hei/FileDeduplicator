using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FileDeduplicator.Models
{
    public partial class FileItem : ObservableObject
    {
        public required string Name { get; set; }
        public required string Path { get; set; }
        public TimeSpan ProcessingTime { get; set; } = TimeSpan.Zero;
        public string HashValue { get; set; } = string.Empty;
        public HashType HashType { get; set; } = HashType.None;

        public CancellationTokenSource ProcessCts { get; set; } = new();

        [ObservableProperty]
        private Brush _backgroundColor = Brushes.Transparent;

        [ObservableProperty]
        private bool _isSelected = false;
    }
}
