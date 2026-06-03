using CommunityToolkit.Mvvm.ComponentModel;

namespace Mp3TagFree.Models
{
    public partial class RenamePreviewItem : ObservableObject
    {
        [ObservableProperty]
        private string _originalName = string.Empty;

        [ObservableProperty]
        private string _newName = string.Empty;

        [ObservableProperty]
        private AudioFile _audioFile;

        [ObservableProperty]
        private string _status = "대기";

        [ObservableProperty]
        private bool _isValid = true;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        public RenamePreviewItem(AudioFile audioFile)
        {
            AudioFile = audioFile;
            OriginalName = audioFile.FileName;
        }
    }
}
