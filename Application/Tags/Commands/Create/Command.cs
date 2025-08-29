using Application.Core;
using Domain.Enums;
using MediatR;

namespace Application.Tags.Commands.Create
{
    public class Command : IRequest<Result<Unit>>
    {
        public TagType Type { get; set; }
        public string Description { get; set; }
        public bool IsPinned { get; set; }
        public Guid OmdbItemId { get; set; }
    }
}