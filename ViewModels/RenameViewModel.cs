using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mp3TagFree.Models;

namespace Mp3TagFree.ViewModels
{
    public partial class RenameViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _pattern = "{아티스트} - {제목} - {발매연도}";

        [ObservableProperty]
        private ObservableCollection<RenamePreviewItem> _previewItems;

        [ObservableProperty]
        private bool _canRename;

        private readonly List<AudioFile> _selectedFiles;

        public RenameViewModel(IEnumerable<AudioFile> selectedFiles)
        {
            _selectedFiles = selectedFiles.ToList();
            _previewItems = new ObservableCollection<RenamePreviewItem>();

            foreach (var file in _selectedFiles)
            {
                _previewItems.Add(new RenamePreviewItem(file));
            }

            UpdatePreviews();
        }

        partial void OnPatternChanged(string value)
        {
            UpdatePreviews();
        }

        private void UpdatePreviews()
        {
            if (PreviewItems == null) return;

            var currentPaths = _selectedFiles.Select(f => f.FilePath).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var targetPaths = new Dictionary<string, List<RenamePreviewItem>>(StringComparer.OrdinalIgnoreCase);

            // Step 1: Calculate new names for each file
            foreach (var item in PreviewItems)
            {
                var file = item.AudioFile;
                string resolved = Pattern;

                // Replace placeholders
                resolved = resolved.Replace("{제목}", file.Title ?? string.Empty);
                resolved = resolved.Replace("{아티스트}", file.Artist ?? string.Empty);
                resolved = resolved.Replace("{앨범}", file.Album ?? string.Empty);
                resolved = resolved.Replace("{발매연도}", file.Year > 0 ? file.Year.ToString() : string.Empty);
                resolved = resolved.Replace("{장르}", file.Genre ?? string.Empty);
                resolved = resolved.Replace("{트랙}", file.Track > 0 ? file.Track.ToString("D2") : "00");

                // Get extension
                string ext = Path.GetExtension(file.FilePath) ?? string.Empty;

                // Sanitize filename
                string sanitizedBase = SanitizeFileName(resolved);
                string newFileName = sanitizedBase + ext;

                item.NewName = newFileName;

                string dir = Path.GetDirectoryName(file.FilePath) ?? string.Empty;
                string newPath = Path.Combine(dir, newFileName);

                item.IsValid = true;
                item.Status = "대기";
                item.ErrorMessage = string.Empty;

                if (string.IsNullOrWhiteSpace(sanitizedBase))
                {
                    item.IsValid = false;
                    item.Status = "이름 없음";
                    item.ErrorMessage = "파일명이 비어있습니다.";
                    continue;
                }

                // Keep track of planned destination paths for conflict detection
                if (!targetPaths.ContainsKey(newPath))
                {
                    targetPaths[newPath] = new List<RenamePreviewItem>();
                }
                targetPaths[newPath].Add(item);
            }

            // Step 2: Validate conflicts and disk existence
            foreach (var kvp in targetPaths)
            {
                string targetPath = kvp.Key;
                var items = kvp.Value;

                if (items.Count > 1)
                {
                    // Multiple files are renaming to the exact same path
                    foreach (var item in items)
                    {
                        item.IsValid = false;
                        item.Status = "중복 오류";
                        item.ErrorMessage = "여러 파일이 동일한 파일명으로 변경됩니다.";
                    }
                }
                else
                {
                    var item = items[0];
                    // Check if file already exists on disk
                    // Only count as conflict if it's not the file's own current path AND it's not the current path of another file being renamed.
                    if (File.Exists(targetPath) && 
                        !item.AudioFile.FilePath.Equals(targetPath, StringComparison.OrdinalIgnoreCase) &&
                        !currentPaths.Contains(targetPath))
                    {
                        item.IsValid = false;
                        item.Status = "파일 존재";
                        item.ErrorMessage = "동일한 이름을 가진 파일이 폴더에 이미 존재합니다.";
                    }
                }
            }

            // Update CanRename property
            CanRename = PreviewItems.Count > 0 && PreviewItems.All(i => i.IsValid);
        }

        private string SanitizeFileName(string name)
        {
            if (string.IsNullOrEmpty(name)) return string.Empty;

            // Replace characters that are invalid in Windows filenames
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c, '_');
            }

            // Clean up consecutive underscores or spaces for better readability
            while (name.Contains("__")) name = name.Replace("__", "_");
            while (name.Contains("  ")) name = name.Replace("  ", " ");

            return name.Trim();
        }

        public bool ExecuteRename()
        {
            if (!CanRename) return false;

            bool anyErrors = false;

            foreach (var item in PreviewItems)
            {
                if (!item.IsValid) continue;

                var file = item.AudioFile;
                string dir = Path.GetDirectoryName(file.FilePath) ?? string.Empty;
                string newPath = Path.Combine(dir, item.NewName);

                // If filename didn't actually change, skip
                if (file.FilePath.Equals(newPath, StringComparison.OrdinalIgnoreCase))
                {
                    item.Status = "변경 없음";
                    continue;
                }

                try
                {
                    if (File.Exists(file.FilePath))
                    {
                        File.Move(file.FilePath, newPath);
                        file.FilePath = newPath;
                        file.FileName = item.NewName;
                        item.Status = "완료";
                    }
                    else
                    {
                        item.IsValid = false;
                        item.Status = "파일 없음";
                        item.ErrorMessage = "원본 파일을 찾을 수 없습니다.";
                        anyErrors = true;
                    }
                }
                catch (Exception ex)
                {
                    item.IsValid = false;
                    item.Status = "오류";
                    item.ErrorMessage = ex.Message;
                    anyErrors = true;
                }
            }

            // Re-evaluate previews after rename
            UpdatePreviews();

            return !anyErrors;
        }
    }
}
