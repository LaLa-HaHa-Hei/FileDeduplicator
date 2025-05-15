using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using FileDeduplicator.Models;
using FileDeduplicator.ViewModels;

namespace FileDeduplicator.Views
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly MainWindowViewModel _mainWindowViewModel = new();
        private ListSortDirection _lastDirection = ListSortDirection.Ascending;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = _mainWindowViewModel;
            // 点击表头事件
            CompletedFilesListView.AddHandler(GridViewColumnHeader.ClickEvent, new RoutedEventHandler(CompletedFilesListViewColumnHeader_Click));
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var items = (string[])e.Data.GetData(DataFormats.FileDrop);
                _mainWindowViewModel.AddFilesByPaths(items);
            }
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Hyperlink link)
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = link.NavigateUri.AbsoluteUri,
                    UseShellExecute = true
                });
            }
        }
        private void CompletedFilesListViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is GridViewColumnHeader headerClicked)
            {
                if (headerClicked.Column.DisplayMemberBinding is not Binding columnBinding)
                    return;
                var direction = _lastDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
                _lastDirection = direction;
                _mainWindowViewModel.SortCompletedFiles(columnBinding?.Path.Path, direction);
            }
        }

        private void RemovePendingFileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (PendingFilesListView.SelectedItem is FileItem selectedItem)
            {
                _mainWindowViewModel.RemovePendingFile(selectedItem);
            }
        }

        private void CancelProcessingFileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (ProcessingFilesListView.SelectedItem is FileItem selectedItem)
            {
                _mainWindowViewModel.CancelProcessingFile(selectedItem);
            }
        }
    }
}
