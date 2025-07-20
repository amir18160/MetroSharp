using Application.Core;
using Domain.Models.Scrapers.Yts;
using MediatR;

namespace Application.Scrapers.Queries.GetAllYTSMovies
{
    public class Query : IRequest<Result<List<YtsPreview>>>
    {
        public string Genre { get; set; }
        public string SortBy { get; set; }
        public int Page { get; set; } = 1;
    }
}