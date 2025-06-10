using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace VictorianTranslator.Services
{
    public class LyricsService : ILyricsService
    {
        private readonly string _lyricsDirectory;
        private const int MaxWords = 200;

        public LyricsService(IWebHostEnvironment webHostEnvironment)
        {
            _lyricsDirectory = Path.Combine(webHostEnvironment.WebRootPath, "scrapes");
            
            // Create directory if it doesn't exist
            if (!Directory.Exists(_lyricsDirectory))
            {
                Directory.CreateDirectory(_lyricsDirectory);
            }
        }

        public async Task<List<string>> GetAvailableSongsAsync()
        {
            if (!Directory.Exists(_lyricsDirectory))
            {
                return new List<string>();
            }

            var files = Directory.GetFiles(_lyricsDirectory, "*.txt")
                               .Select(Path.GetFileName)
                               .OrderBy(f => f)
                               .ToList();
            return await Task.FromResult(files);
        }

        public async Task<string> GetLyricsAsync(string songFileName)
        {
            var filePath = Path.Combine(_lyricsDirectory, songFileName);
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Lyrics file not found: {songFileName}");
            }

            var fullText = await File.ReadAllTextAsync(filePath);
            var words = fullText.Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            
            if (words.Length <= MaxWords)
            {
                return fullText;
            }

            return string.Join(" ", words.Take(MaxWords)) + "...";
        }
    }
} 