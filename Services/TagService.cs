using System;
using System.IO;
using System.Text;
using Mp3TagFree.Models;

namespace Mp3TagFree.Services
{
    public class TagService : ITagService
    {
        static TagService()
        {
            // Register encoding provider to support EUC-KR / CP949 encoding
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public AudioFile ReadTag(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            if (!fileInfo.Exists)
            {
                throw new FileNotFoundException("File not found", filePath);
            }

            var audioFile = new AudioFile
            {
                FilePath = filePath,
                FileName = fileInfo.Name,
                FileSize = fileInfo.Length,
                IsDirty = false
            };

            try
            {
                using (var tagFile = TagLib.File.Create(filePath))
                {
                    // Duration formatting (e.g. 03:45)
                    var durationTime = tagFile.Properties.Duration;
                    audioFile.Duration = $"{(int)durationTime.TotalMinutes:D2}:{durationTime.Seconds:D2}";

                    if (tagFile.Tag != null)
                    {
                        audioFile.Title = AutoDecode(tagFile.Tag.Title);
                        audioFile.Artist = AutoDecode(tagFile.Tag.FirstPerformer);
                        audioFile.Album = AutoDecode(tagFile.Tag.Album);
                        audioFile.Year = tagFile.Tag.Year;
                        audioFile.Genre = AutoDecode(tagFile.Tag.FirstGenre);
                        audioFile.Track = tagFile.Tag.Track;
                        audioFile.Lyrics = AutoDecode(tagFile.Tag.Lyrics);

                        // Read album art
                        if (tagFile.Tag.Pictures != null && tagFile.Tag.Pictures.Length > 0)
                        {
                            audioFile.AlbumArt = tagFile.Tag.Pictures[0].Data.Data;
                        }
                    }
                }
            }
            catch (Exception)
            {
                // If parsing fails, fall back to filename as title and set other fields empty
                audioFile.Title = Path.GetFileNameWithoutExtension(fileInfo.Name);
                audioFile.Artist = "Unknown Artist";
                audioFile.Album = "Unknown Album";
                audioFile.Duration = "00:00";
            }

            // Clear dirty state because setting properties triggers the dirty flag
            audioFile.ClearDirty();
            return audioFile;
        }

        public void WriteTag(AudioFile audioFile)
        {
            if (!File.Exists(audioFile.FilePath))
            {
                throw new FileNotFoundException("File not found", audioFile.FilePath);
            }

            using (var tagFile = TagLib.File.Create(audioFile.FilePath))
            {
                if (tagFile.Tag != null)
                {
                    tagFile.Tag.Title = audioFile.Title;
                    tagFile.Tag.Performers = new[] { audioFile.Artist };
                    tagFile.Tag.Album = audioFile.Album;
                    tagFile.Tag.Year = audioFile.Year;
                    tagFile.Tag.Genres = new[] { audioFile.Genre };
                    tagFile.Tag.Track = audioFile.Track;
                    tagFile.Tag.Lyrics = audioFile.Lyrics;

                    // Write album art
                    if (audioFile.AlbumArt != null)
                    {
                        var pic = new TagLib.Picture(new TagLib.ByteVector(audioFile.AlbumArt))
                        {
                            Type = TagLib.PictureType.FrontCover,
                            Description = "Cover"
                        };
                        tagFile.Tag.Pictures = new TagLib.IPicture[] { pic };
                    }
                    else
                    {
                        tagFile.Tag.Pictures = Array.Empty<TagLib.IPicture>();
                    }
                }

                tagFile.Save();
            }

            audioFile.ClearDirty();
        }

        /// <summary>
        /// Heuristic to fix broken encodings (e.g. EUC-KR bytes read as ISO-8859-1 / Latin-1)
        /// </summary>
        private string AutoDecode(string? input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            try
            {
                // Check if the input contains characters that are typical of Latin-1 representations of EUC-KR
                // e.g., if we convert to ISO-8859-1 and decode as EUC-KR, we check if it results in valid Korean characters.
                byte[] latinBytes = Encoding.GetEncoding("ISO-8859-1").GetBytes(input);
                string decoded = Encoding.GetEncoding("euc-kr").GetString(latinBytes);

                // Count Hangul syllables (0xAC00 to 0xD7A3)
                int hangulCount = 0;
                foreach (char c in decoded)
                {
                    if (c >= 0xAC00 && c <= 0xD7A3)
                    {
                        hangulCount++;
                    }
                }

                // If decoded string has Hangul, it was likely mis-encoded as ISO-8859-1
                if (hangulCount > 0)
                {
                    return decoded;
                }
            }
            catch
            {
                // Ignore conversion errors and use original input
            }

            return input;
        }
    }
}
