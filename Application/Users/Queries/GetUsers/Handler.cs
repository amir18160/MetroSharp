using Application.Core;
using MediatR;
using Persistence;

namespace Application.Users.Queries.GetUsers
{
    public class Handler : IRequestHandler<Query, Result<PagedList<UserBase>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<PagedList<UserBase>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = from user in _context.Users
                        join userRole in _context.UserRoles on user.Id equals userRole.UserId into urj
                        from userRole in urj.DefaultIfEmpty()
                        join role in _context.Roles on userRole.RoleId equals role.Id into rj
                        from role in rj.DefaultIfEmpty()
                        select new { user, role.Name };

            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                query = query.Where(x => x.user.Name.Contains(request.Name));
            }

            if (!string.IsNullOrWhiteSpace(request.UserName))
            {
                query = query.Where(x => x.user.UserName.Contains(request.UserName));
            }

            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                query = query.Where(x => x.user.Email.Contains(request.Email));
            }

            if (!string.IsNullOrWhiteSpace(request.Role))
            {
                query = query.Where(x => x.Name == request.Role);
            }

            if (request.IsActive.HasValue)
            {
                query = query.Where(x => x.user.IsActive == request.IsActive);
            }

            if (request.IsConfirmed.HasValue)
            {
                query = query.Where(x => x.user.IsConfirmed == request.IsConfirmed);
            }

            if (request.CreatedAt != null)
            {
                query = query.ApplyDateFilter(request?.CreatedAt, x => x.user.CreatedAt);
            }


            var projected = query.Select(x => new UserBase
            {
                Id = x.user.Id,
                Username = x.user.UserName,
                Email = x.user.Email,
                Name = x.user.Name,
                IsActive = x.user.IsActive,
                IsConfirmed = x.user.IsConfirmed,
                CreatedAt = x.user.CreatedAt,
                Role = x.Name
            }).AsQueryable();

            var pagedList = await PagedList<UserBase>.CreateAsync(
                projected,
                request.PageNumber,
                request.PageSize);

            return Result<PagedList<UserBase>>.Success(pagedList);
        }
    }
}
