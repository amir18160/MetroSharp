using Application.Core;
using MediatR;

namespace Application.BannedUsers.Queries.GetBannedUsers
{
    public class Query : PagingParams,  IRequest<Result<PagedList<BannedUserDto>>>
    {
        public string UserName { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string UserId { get; set; }
        public bool? IsIndefinite { get; set; }
        public DateFilter BannedAt { get; set; }
        public DateFilter FreeAt { get; set; }
    }
}