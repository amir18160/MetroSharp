using Application.Core;
using MediatR;

namespace Application.Tags.Queries.GetTags;

public class Query : IRequest<Result<TagsFeedDto>>
{
    public bool? IncludeOmdbItem { get; set; }
}