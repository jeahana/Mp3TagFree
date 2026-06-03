using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Mp3TagFree.Models;
using Mp3TagFree.ViewModels;

namespace Mp3TagFree
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;

        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
        }

        // Synchronize selected files to ViewModel
        private void FilesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FilesDataGrid.SelectedItems != null)
            {
                var selectedList = FilesDataGrid.SelectedItems.Cast<AudioFile>().ToList();
                _viewModel.HandleSelectionChanged(selectedList);
            }
        }

        // Drag & Drop files into the MainWindow
        private void Window_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private async void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var paths = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (paths != null && paths.Length > 0)
                {
                    // Filter directories vs files
                    var fileList = paths.SelectMany(path =>
                    {
                        if (Directory.Exists(path))
                        {
                            var allowedExtensions = new[] { ".mp3", ".flac", ".m4a", ".wav", ".ogg" };
                            return Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
                                            .Where(f => allowedExtensions.Contains(Path.GetExtension(f).ToLower()));
                        }
                        else
                        {
                            return new[] { path };
                        }
                    }).Where(f => File.Exists(f)).ToList();

                    if (fileList.Count > 0)
                    {
                        await _viewModel.LoadFilesAsync(fileList);
                    }
                }
            }
        }

        // Drag & Drop album art images into the album art container
        private void AlbumArt_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Length > 0)
                {
                    var ext = Path.GetExtension(files[0]).ToLower();
                    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png")
                    {
                        e.Effects = DragDropEffects.Copy;
                        e.Handled = true;
                        return;
                    }
                }
            }
            e.Effects = DragDropEffects.None;
            e.Handled = true;
        }

        private void AlbumArt_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Length > 0)
                {
                    try
                    {
                        var ext = Path.GetExtension(files[0]).ToLower();
                        if (ext == ".jpg" || ext == ".jpeg" || ext == ".png")
                        {
                            _viewModel.AlbumArtInput = File.ReadAllBytes(files[0]);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"이미지 로드 실패: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void DataGridRow_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is DataGridRow row && row.Item is AudioFile audioFile)
            {
                try
                {
                    if (File.Exists(audioFile.FilePath))
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = audioFile.FilePath,
                            UseShellExecute = true
                        });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"파일을 재생할 수 없습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}