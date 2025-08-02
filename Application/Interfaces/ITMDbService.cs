using Domain.Models.TMDb;
using Domain.Models.TMDb.General;

namespace Application.Interfaces
{
    public interface ITMDbService
    {
        Task<List<TMDbMedia>> SearchMulti(MultiSearchParams @params);
        Task<Domain.Models.TMDb.Movies.Movie> GetMovieDetails(GetMediaDetailsParams @params);
        Task<Domain.Models.TMDb.TvShows.TvShow> GetTvShowDetails(GetMediaDetailsParams @params);
    }
}