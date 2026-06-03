using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mp3TagFree.Models;
using Mp3TagFree.Services;
using Mp3TagFree;

namespace Mp3TagFree.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly ITagService _tagService;
        private bool _isUpdatingInputs;

        public MainViewModel(ITagService tagService)
        {
            _tagService = tagService;
            Files = new ObservableCollection<AudioFile>();
            SelectedFiles = new ObservableCollection<AudioFile>();
        }

        [ObservableProperty]
        private ObservableCollection<AudioFile> _files;

        [ObservableProperty]
        private ObservableCollection<AudioFile> _selectedFiles;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _loadingStatus = string.Empty;

        // Input fields for editing
        [ObservableProperty] private string _titleInput = string.Empty;
        [ObservableProperty] private string _artistInput = string.Empty;
        [ObservableProperty] private string _albumInput = string.Empty;
        [ObservableProperty] private string _yearInput = string.Empty;
        [ObservableProperty] private string _genreInput = string.Empty;
        [ObservableProperty] private string _trackInput = string.Empty;
        [ObservableProperty] private string _lyricsInput = string.Empty;
        [ObservableProperty] private byte[]? _albumArtInput;

        // Flags to track user changes during multi-select
        private bool _isTitleModified;
        private bool _isArtistModified;
        private bool _isAlbumModified;
        private bool _isYearModified;
        private bool _isGenreModified;
        private bool _isTrackModified;
        private bool _isLyricsModified;
        private bool _isAlbumArtModified;

        public bool IsSingleSelected => SelectedFiles.Count == 1;
        public bool IsMultiSelected => SelectedFiles.Count > 1;
        public bool IsAnySelected => SelectedFiles.Count >= 1;

        public string SelectionStatusText
        {
            get
            {
                if (SelectedFiles.Count == 0) return "선택된 파일 없음";
                return $"{SelectedFiles.Count}개의 파일 선택됨";
            }
        }

        partial void OnTitleInputChanged(string value) { if (!_isUpdatingInputs) _isTitleModified = true; }
        partial void OnArtistInputChanged(string value) { if (!_isUpdatingInputs) _isArtistModified = true; }
        partial void OnAlbumInputChanged(string value) { if (!_isUpdatingInputs) _isAlbumModified = true; }
        partial void OnYearInputChanged(string value) { if (!_isUpdatingInputs) _isYearModified = true; }
        partial void OnGenreInputChanged(string value) { if (!_isUpdatingInputs) _isGenreModified = true; }
        partial void OnTrackInputChanged(string value) { if (!_isUpdatingInputs) _isTrackModified = true; }
        partial void OnLyricsInputChanged(string value) { if (!_isUpdatingInputs) _isLyricsModified = true; }
        partial void OnAlbumArtInputChanged(byte[]? value) { if (!_isUpdatingInputs) _isAlbumArtModified = true; }

        public void HandleSelectionChanged(IEnumerable<AudioFile> selected)
        {
            SelectedFiles.Clear();
            foreach (var file in selected)
            {
                SelectedFiles.Add(file);
            }

            OnPropertyChanged(nameof(IsSingleSelected));
            OnPropertyChanged(nameof(IsMultiSelected));
            OnPropertyChanged(nameof(IsAnySelected));
            OnPropertyChanged(nameof(SelectionStatusText));

            UpdateInputsFromSelection();
        }

        private void UpdateInputsFromSelection()
        {
            _isUpdatingInputs = true;

            // Reset modification flags
            _isTitleModified = false;
            _isArtistModified = false;
            _isAlbumModified = false;
            _isYearModified = false;
            _isGenreModified = false;
            _isTrackModified = false;
            _isLyricsModified = false;
            _isAlbumArtModified = false;

            if (SelectedFiles.Count == 1)
            {
                var file = SelectedFiles[0];
                TitleInput = file.Title;
                ArtistInput = file.Artist;
                AlbumInput = file.Album;
                YearInput = file.Year > 0 ? file.Year.ToString() : string.Empty;
                GenreInput = file.Genre;
                TrackInput = file.Track > 0 ? file.Track.ToString() : string.Empty;
                LyricsInput = file.Lyrics;
                AlbumArtInput = file.AlbumArt;
            }
            else if (SelectedFiles.Count > 1)
            {
                // Multi-selection: show common values or placeholders
                var titles = SelectedFiles.Select(f => f.Title).Distinct().ToList();
                TitleInput = titles.Count == 1 ? titles[0] : "<여러 제목 존재>";

                var artists = SelectedFiles.Select(f => f.Artist).Distinct().ToList();
                ArtistInput = artists.Count == 1 ? artists[0] : "<여러 아티스트 존재>";

                var albums = SelectedFiles.Select(f => f.Album).Distinct().ToList();
                AlbumInput = albums.Count == 1 ? albums[0] : "<여러 앨범 존재>";

                var years = SelectedFiles.Select(f => f.Year).Distinct().ToList();
                YearInput = years.Count == 1 ? (years[0] > 0 ? years[0].ToString() : string.Empty) : "<여러 연도 존재>";

                var genres = SelectedFiles.Select(f => f.Genre).Distinct().ToList();
                GenreInput = genres.Count == 1 ? genres[0] : "<여러 장르 존재>";

                var tracks = SelectedFiles.Select(f => f.Track).Distinct().ToList();
                TrackInput = tracks.Count == 1 ? (tracks[0] > 0 ? tracks[0].ToString() : string.Empty) : "<여러 트랙 존재>";

                var lyricsList = SelectedFiles.Select(f => f.Lyrics).Distinct().ToList();
                LyricsInput = lyricsList.Count == 1 ? lyricsList[0] : "<여러 가사 존재>";

                // Check if all selected files have the same album art (by comparing length or reference)
                var arts = SelectedFiles.Select(f => f.AlbumArt).ToList();
                bool allSameArt = true;
                if (arts.Count > 0)
                {
                    var firstArt = arts[0];
                    foreach (var art in arts)
                    {
                        if (firstArt == null && art != null || firstArt != null && art == null)
                        {
                            allSameArt = false;
                            break;
                        }
                        if (firstArt != null && art != null && firstArt.Length != art.Length)
                        {
                            allSameArt = false;
                            break;
                        }
                    }
                }
                AlbumArtInput = allSameArt ? arts[0] : null;
            }
            else
            {
                // No selection: clear inputs
                TitleInput = string.Empty;
                ArtistInput = string.Empty;
                AlbumInput = string.Empty;
                YearInput = string.Empty;
                GenreInput = string.Empty;
                TrackInput = string.Empty;
                LyricsInput = string.Empty;
                AlbumArtInput = null;
            }

            _isUpdatingInputs = false;
        }

        [RelayCommand]
        private async Task OpenFilesAsync()
        {
            var dialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "오디오 파일 (*.mp3;*.flac;*.m4a;*.wav;*.ogg)|*.mp3;*.flac;*.m4a;*.wav;*.ogg|모든 파일 (*.*)|*.*",
                Title = "오디오 파일 추가"
            };

            if (dialog.ShowDialog() == true)
            {
                await LoadFilesAsync(dialog.FileNames);
            }
        }

        [RelayCommand]
        private async Task OpenFolderAsync()
        {
            var dialog = new OpenFolderDialog
            {
                Title = "오디오 폴더 선택"
            };

            if (dialog.ShowDialog() == true)
            {
                string folder = dialog.FolderName;
                var allowedExtensions = new[] { ".mp3", ".flac", ".m4a", ".wav", ".ogg" };
                
                IsLoading = true;
                LoadingStatus = "폴더 내 오디오 파일을 찾는 중...";

                var filePaths = await Task.Run(() =>
                {
                    try
                    {
                        return Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories)
                                        .Where(path => allowedExtensions.Contains(Path.GetExtension(path).ToLower()))
                                        .ToArray();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"폴더 읽기 오류: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                        return Array.Empty<string>();
                    }
                });

                if (filePaths.Length > 0)
                {
                    await LoadFilesAsync(filePaths);
                }
                else
                {
                    IsLoading = false;
                    MessageBox.Show("폴더 내에 지원되는 오디오 파일이 없습니다.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        public async Task LoadFilesAsync(IEnumerable<string> paths)
        {
            IsLoading = true;
            LoadingStatus = "태그 정보를 읽는 중...";

            var newFiles = new List<AudioFile>();

            await Task.Run(() =>
            {
                foreach (var path in paths)
                {
                    // Check if file is already loaded to avoid duplicates
                    if (Files.Any(f => f.FilePath.Equals(path, StringComparison.OrdinalIgnoreCase)))
                        continue;

                    try
                    {
                        var audio = _tagService.ReadTag(path);
                        newFiles.Add(audio);
                    }
                    catch
                    {
                        // Skip corrupted files
                    }
                }
            });

            foreach (var audio in newFiles)
            {
                Files.Add(audio);
            }

            IsLoading = false;
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (SelectedFiles.Count == 0) return;

            IsLoading = true;
            LoadingStatus = "태그 정보를 파일에 저장하는 중...";

            // Parsing inputs
            uint.TryParse(YearInput, out uint yearVal);
            uint.TryParse(TrackInput, out uint trackVal);

            var filesToSave = SelectedFiles.ToList();

            await Task.Run(() =>
            {
                foreach (var file in filesToSave)
                {
                    bool isFileDirty = false;

                    if (SelectedFiles.Count == 1)
                    {
                        // Single select: apply all inputs
                        file.Title = TitleInput;
                        file.Artist = ArtistInput;
                        file.Album = AlbumInput;
                        file.Year = yearVal;
                        file.Genre = GenreInput;
                        file.Track = trackVal;
                        file.Lyrics = LyricsInput;
                        file.AlbumArt = AlbumArtInput;
                        isFileDirty = true;
                    }
                    else
                    {
                        // Multi select: apply only modified fields
                        if (_isTitleModified && TitleInput != "<여러 제목 존재>") { file.Title = TitleInput; isFileDirty = true; }
                        if (_isArtistModified && ArtistInput != "<여러 아티스트 존재>") { file.Artist = ArtistInput; isFileDirty = true; }
                        if (_isAlbumModified && AlbumInput != "<여러 앨범 존재>") { file.Album = AlbumInput; isFileDirty = true; }
                        if (_isYearModified && YearInput != "<여러 연도 존재>") { file.Year = yearVal; isFileDirty = true; }
                        if (_isGenreModified && GenreInput != "<여러 장르 존재>") { file.Genre = GenreInput; isFileDirty = true; }
                        if (_isTrackModified && TrackInput != "<여러 트랙 존재>") { file.Track = trackVal; isFileDirty = true; }
                        if (_isLyricsModified && LyricsInput != "<여러 가사 존재>") { file.Lyrics = LyricsInput; isFileDirty = true; }
                        if (_isAlbumArtModified) { file.AlbumArt = AlbumArtInput; isFileDirty = true; }
                    }

                    if (isFileDirty)
                    {
                        try
                        {
                            _tagService.WriteTag(file);
                        }
                        catch (Exception ex)
                        {
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                MessageBox.Show($"파일 저장 실패 ({Path.GetFileName(file.FilePath)}): {ex.Message}", 
                                    "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                            });
                        }
                    }
                }
            });

            // Refresh input fields from selection to clear placeholders or update preview
            UpdateInputsFromSelection();

            IsLoading = false;
        }

        [RelayCommand]
        private void ChangeAlbumArt()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "이미지 파일 (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png",
                Title = "앨범 아트 이미지 선택"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    AlbumArtInput = File.ReadAllBytes(dialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"이미지 로드 실패: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        [RelayCommand]
        private void RemoveAlbumArt()
        {
            AlbumArtInput = null;
        }

        [RelayCommand]
        private void RemoveSelected()
        {
            var itemsToRemove = SelectedFiles.ToList();
            foreach (var item in itemsToRemove)
            {
                Files.Remove(item);
            }
            SelectedFiles.Clear();
            UpdateInputsFromSelection();
        }

        [RelayCommand]
        private void ClearList()
        {
            Files.Clear();
            SelectedFiles.Clear();
            UpdateInputsFromSelection();
        }

        [RelayCommand]
        private void RenameFiles()
        {
            if (SelectedFiles.Count == 0) return;

            var renameVm = new RenameViewModel(SelectedFiles);
            var dialog = new RenameWindow(renameVm)
            {
                Owner = Application.Current.MainWindow
            };

            if (dialog.ShowDialog() == true)
            {
                OnPropertyChanged(nameof(SelectionStatusText));
            }
        }
    }
}
