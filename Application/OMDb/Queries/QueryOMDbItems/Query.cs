using Application.Core;
using Application.OMDb.DTOs;
using Domain.Entities;
using Domain.Enums;
using MediatR;

namespace Application.OMDb.Queries.QueryOMDbItems;

public class Query : PagingParams, IRequest<Result<PagedList<OmdbItemDto>>>
{
    public string Title { get; set; }
    public string ImdbId { get; set; }
    public string Genre { get; set; }
    public TagType? TagType { get; set; }
    public bool IncludeTags { get; set; }
    public bool IncludeDocuments { get; set; }
    public bool IncludeSeasons { get; set; }
}