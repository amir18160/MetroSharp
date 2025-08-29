using Application.Core;
using Domain.Entities;
using MediatR;

namespace Application.OMDb.Queries.QueryOMDbItems
{
    public class Query : PagingParams, IRequest<Result<PagedList<OmdbItem>>>
    {
        public string Title { get; set; }
        public string ImdbId { get; set; }
        public bool IncludeTags { get; set; }
        public bool IncludeDocuments { get; set; }
        public bool IncludeSeasons { get; set; }
    }
}