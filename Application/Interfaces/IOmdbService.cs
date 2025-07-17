using Domain.Entities;

namespace Application.Interfaces
{
    public interface IOmdbService
    {
        Task<OmdbItem> GetTitleByImdbIdAsync(string imdbId);
    }
}