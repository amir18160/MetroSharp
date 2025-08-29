using Application.Core;
using Domain.Entities;
using Domain.Enums;
using MediatR;

namespace Application.Tags.Queries.GetTags
{
    public class Query :  IRequest<Result<TagsFeedDto>>
    {
        public bool? IncludeOmdbItem  { get; set; }
    }
}