using Application.Core;
using Domain.Models.Scrapers.X1337;
using Domain.Models.Scrapers.Yts;
using MediatR;

namespace Application.Scrapers.Commands.SearchYTS
{
    public class Command : IRequest<Result<List<YtsPreview>>>
    {
        public string SearchTerm { get; set; }
    }
}