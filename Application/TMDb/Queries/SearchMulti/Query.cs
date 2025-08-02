using Application.Core;
using Domain.Models.TMDb;
using Domain.Models.TMDb.General;
using MediatR;

namespace Application.TMDb.Queries.SearchMulti
{
    public class Query : IRequest<Result<List<TMDbMedia>>>
    {
        public string SearchQuery { get; set; }
        public int Year { get; set; }
        public int Page { get; set; }
    }
}