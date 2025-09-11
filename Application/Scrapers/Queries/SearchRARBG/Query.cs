using Application.Core;
using Domain.Models.Scrapers.Rarbg;
using MediatR;

namespace Application.Scrapers.Queries.SearchRarbg;

public class Query : IRequest<Result<List<RarbgPreview>>>
{
    public string SearchTerm { get; set; }
}