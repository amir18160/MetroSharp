using Domain.Models.TMDb.General;
using Domain.Models.TMDb.Movies;
using Domain.Models.TMDb.TvShows;

namespace Application.Interfaces;

public interface ITMDbService
{
    Task<List<TMDbMedia>> SearchMulti(MultiSearchParams @params);
    Task<Movie> GetMovieDetails(GetMediaDetailsParams @params);
    Task<TvShow> GetTvShowDetails(GetMediaDetailsParams @params);
}