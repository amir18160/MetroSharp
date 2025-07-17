using Application.Core;
using Application.Interfaces;
using MediatR;

namespace Application.Users.Queries.GetUsers
{
    public class Query : PagingParams, IRequest<Result<PagedList<UserBase>>>
    {
        public string Name { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string Phone { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsConfirmed { get; set; }
        public DateFilter CreatedAt { get; set; }
    }
}
