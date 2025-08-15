using Application.Core;
using Domain.Models.Scrapers.X1337;
using Domain.Models.Scrapers.Yts;
using MediatR;

namespace Application.Scrapers.Queries.SearchYTS
{
    public class Query : IRequest<Result<List<YtsPreview>>>
    {
        public string SearchTerm { get; set; }
    }
}