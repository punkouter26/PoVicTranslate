using System.Collections.Generic;
using System.Threading.Tasks;

namespace Po.VicTranslate.Api.Services;

public interface ILyricsService
{
    Task<List<string>> GetAvailableSongsAsync();
    Task<string> GetLyricsAsync(string songFileName);
}
