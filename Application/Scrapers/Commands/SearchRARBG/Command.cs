using Application.Core;
using Domain.Models.Scrapers.Rarbg;
using Domain.Models.Scrapers.X1337;
using MediatR;

namespace Application.Scrapers.Commands.SearchRarbg
{
    public class Command : IRequest<Result<List<RarbgPreview>>>
    {
        public string SearchTerm { get; set; }
    }
}