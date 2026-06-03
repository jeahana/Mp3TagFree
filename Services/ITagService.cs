using Mp3TagFree.Models;

namespace Mp3TagFree.Services
{
    public interface ITagService
    {
        AudioFile ReadTag(string filePath);
        void WriteTag(AudioFile audioFile);
    }
}
