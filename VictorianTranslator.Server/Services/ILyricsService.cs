using System.Collections.Generic;
using System.Threading.Tasks;

namespace VictorianTranslator.Services
{
    public interface ILyricsService
    {
        Task<List<string>> GetAvailableSongsAsync();
        Task<string> GetLyricsAsync(string songFileName);
    }
} 