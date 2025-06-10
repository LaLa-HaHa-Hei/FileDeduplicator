using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDeduplicator.Models;
using FileDeduplicator.Services;

namespace FileDeduplicator.ViewModels
{

    public partial class MainWindowViewModel : ObservableObject
    {
        public ObservableCollection<FileItem> PendingFiles { get; } = []; // 待处理文件列表
        public ObservableCollection<FileItem> ProcessingFiles { get; } = []; // 正在处理文件列表
        public ObservableCollection<FileItem> CompletedFiles { get; } = []; // 完成处理文件列表
        public int PendingFilesCount => PendingFiles.Count; // 待处理文件数量
        public int ProcessingFilesCount => ProcessingFiles.Count; // 正在处理文件数量
        public int CompletedFilesCount => CompletedFiles.Count; // 完成处理文件数量
        public List<int> ConcurrentFileCountOptions { get; } = [1, 2, 4, 8, 16, 32];
        [ObservableProperty]
        private int _concurrentFileCount = 4; // 默认并发处理文件数量
        public List<HashType> HashTypes { get; } = // 哈希类型列表
        [
            HashType.MD5,
            HashType.SHA1,
            HashType.SHA256,
            HashType.SHA384,
            HashType.SHA512
        ];
        [ObservableProperty]
        private HashType _hashType = HashType.MD5; // 哈希类型

        private readonly BackgroundColorServices _backgroundColorServices = new(); // 背景颜色

        public MainWindowViewModel()
        {
            PendingFiles.CollectionChanged += (s, e) => OnPropertyChanged(nameof(PendingFilesCount));
            ProcessingFiles.CollectionChanged += (s, e) => OnPropertyChanged(nameof(ProcessingFilesCount));
            CompletedFiles.CollectionChanged += (s, e) => OnPropertyChanged(nameof(CompletedFilesCount));
        }

        partial void OnConcurrentFileCountChanged(int value) => ProcessFiles(); // 当并发文件数量改变时，重新处理文件

        [RelayCommand]
        private void SelectDuplicateFiles()
        {
            var groupedFiles = CompletedFiles
                .GroupBy(f => f.HashValue)
                .Where(g => g.Count() > 1);
            foreach (var group in groupedFiles)
            {
                // 保留每组第一个文件未选中，其余设为选中
                foreach (var file in group.Skip(1))
                {
                    file.IsSelected = true;
                }
            }
        }

        [RelayCommand]
        private void DeleteSelectedFilesToRecycleBin()
        {
            for (var i = CompletedFilesCount - 1; i >= 0; i--)
            {
                var file = CompletedFiles[i];
                if (file.IsSelected)
                {
                    DeleteFileToRecycleBin(file.Path);
                    CompletedFiles.RemoveAt(i);
                }
            }
        }

        [RelayCommand]
        private void RemoveSelectedFilesFromList()
        {
            for (var i = CompletedFilesCount - 1; i >= 0; i--)
            {
                var file = CompletedFiles[i];
                if (file.IsSelected)
                    CompletedFiles.RemoveAt(i);
            }
        }

        public void RemovePendingFile(FileItem fileItem)
        {
            if (PendingFiles.Contains(fileItem))
            {
                PendingFiles.Remove(fileItem);
            }
        }

        public void CancelProcessingFile(FileItem fileItem)
        {
            if (ProcessingFiles.Contains(fileItem))
            {
                fileItem.ProcessCts.Cancel(); // 取消处理
                ProcessingFiles.Remove(fileItem);
                ProcessFiles();
            }
        }

        public void SortCompletedFiles(string sortBy, ListSortDirection direction)
        {
            if (string.IsNullOrEmpty(sortBy))
                return;
            var collectionView = CollectionViewSource.GetDefaultView(CompletedFiles);
            collectionView.SortDescriptions.Clear();
            collectionView.SortDescriptions.Add(new SortDescription(sortBy, direction));
        }

        private static void DeleteFileToRecycleBin(string filePath) =>
            Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(filePath, Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs, Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);

        public void AddFilesByPaths(string[] paths)
        {
            foreach (var path in paths)
            {
                if (File.Exists(path))
                {
                    if (PendingFiles.Any(f => f.Path == path) || ProcessingFiles.Any(f => f.Path == path) || CompletedFiles.Any(f => f.Path == path))
                        continue;
                    PendingFiles.Add(new FileItem() { Name = Path.GetFileName(path), Path = path });
                }
                else if (Directory.Exists(path))
                {
                    // 递归获取所有文件（包括子目录）
                    foreach (var filePath in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
                    {
                        if (PendingFiles.Any(f => f.Path == filePath) || ProcessingFiles.Any(f => f.Path == filePath) || CompletedFiles.Any(f => f.Path == filePath))
                            continue;
                        PendingFiles.Add(new FileItem() { Name = Path.GetFileName(filePath), Path = filePath });
                    }
                }
            }

            // 处理文件
            ProcessFiles();
        }

        public void ProcessFiles()
        {
            while (PendingFilesCount > 0 && ConcurrentFileCount > ProcessingFilesCount)
            {
                var fileItem = PendingFiles[0];
                PendingFiles.RemoveAt(0);
                ProcessingFiles.Add(fileItem);
                var random = new Random();
                Task.Run(async () =>
                {
                    fileItem.HashType = HashType; // 设置哈希类型
                    var stopwatch = Stopwatch.StartNew(); // 开始计时

                    await CalculateHash(fileItem); // 计算哈希

                    stopwatch.Stop(); // 停止计时

                    fileItem.ProcessingTime = stopwatch.Elapsed; // 记录花费的时间
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ProcessingFiles.Remove(fileItem);
                        SetBackgroundColor(fileItem);
                        CompletedFiles.Add(fileItem);
                        ProcessFiles();
                    });
                }, fileItem.ProcessCts.Token);
            }
        }

        // 计算哈希
        private static async Task CalculateHash(FileItem fileItem)
        {
            byte[] hashBytes = [];
            switch (fileItem.HashType)
            {
                case HashType.MD5:
                    hashBytes = await HashCalculatorService.ComputeFileMd5Async(fileItem.Path, fileItem.ProcessCts.Token);
                    break;
                case HashType.SHA1:
                    hashBytes = await HashCalculatorService.ComputeFileSha1Async(fileItem.Path, fileItem.ProcessCts.Token);
                    break;
                case HashType.SHA256:
                    hashBytes = await HashCalculatorService.ComputeFileSha256Async(fileItem.Path, fileItem.ProcessCts.Token);
                    break;
                case HashType.SHA384:
                    hashBytes = await HashCalculatorService.ComputeFileSha384Async(fileItem.Path, fileItem.ProcessCts.Token);
                    break;
                case HashType.SHA512:
                    hashBytes = await HashCalculatorService.ComputeFileSha512Async(fileItem.Path, fileItem.ProcessCts.Token);
                    break;
                default:
                    break;
            }
            fileItem.HashValue = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }

        private void SetBackgroundColor(FileItem fileItem)
        {
            // 如何之前有相同哈希的文件，则使用相同背景色（透明色以外）
            var existingItem = CompletedFiles.FirstOrDefault(f => f.HashValue == fileItem.HashValue);
            if (existingItem != null)
            {
                if (existingItem.BackgroundColor == Brushes.Transparent)
                {
                    // 获取新的一种颜色
                    var newColor = _backgroundColorServices.GetNextColor();
                    existingItem.BackgroundColor = newColor;
                    fileItem.BackgroundColor = newColor;
                }
                else
                {
                    // 跟已有的元素使用相同颜色
                    fileItem.BackgroundColor = existingItem.BackgroundColor;
                }
            }
        }
    }
}
