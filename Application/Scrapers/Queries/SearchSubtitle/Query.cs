using Application.Core;
using Domain.Models.Scrapers.Common;
using MediatR;

namespace Application.Scrapers.Queries.SearchSubtitle
{
    public class Query: IRequest<Result<List<SubtitleSearch>>>
    {
        public string SearchQuery { get; set; }
    }
}