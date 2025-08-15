using Application.Core;
using Domain.Models.Scrapers.X1337;
using MediatR;

namespace Application.Scrapers.Queries.Search1337x
{
    public class Query : IRequest<Result<List<X1337Preview>>>
    {
        public string SearchTerm { get; set; }
    }
}