using Application.Core;
using MediatR;

namespace Application.OMDb.Commands.Update
{
    public class Command : IRequest<Result<Unit>>
    {
        public string Id { get; set; }
        public UpdateOmdbItemDto OmdbItem { get; set; }
    }
}