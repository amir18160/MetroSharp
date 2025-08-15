using Application.Core;
using Domain.Models.Scrapers.Common;
using MediatR;

namespace Application.Scrapers.Queries.GetSubtitles
{
    public class Query : IRequest<Result<List<SubtitleListItem>>>
    {
        public string URL { get; set; }
    }
}