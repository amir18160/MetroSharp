using Application.Core;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Users.Queries.GetSingleUser
{
    public class Handler : IRequestHandler<Query, Result<UserDetailsDto>>
    {
        private readonly DataContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;

        public Handler(DataContext context, UserManager<User> userManager, IMapper mapper)
        {
            _context = context;
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<Result<UserDetailsDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            User user = null;

            if (!string.IsNullOrWhiteSpace(request.Id))
            {
                user = await _context.Users.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            }
            else if (!string.IsNullOrWhiteSpace(request.TelegramId))
            {
                user = await _context.Users.FirstOrDefaultAsync(x => x.TelegramId == request.TelegramId, cancellationToken);
            }
            else if (!string.IsNullOrWhiteSpace(request.Phone))
            {
                user = await _context.Users.FirstOrDefaultAsync(x => x.PhoneNumber == request.Phone, cancellationToken);
            }

            if (user == null)
            {
                return Result<UserDetailsDto>.Failure("User not found");
            }

            var isBanned = await _context.BannedUsers
                .AnyAsync(x => x.UserId == user.Id && (x.FreeAt == null || x.FreeAt > DateTime.UtcNow), cancellationToken);

            var hasSubscription = await _context.Subscriptions
                .AnyAsync(x => x.UserId == user.Id && x.EndAt > DateTime.UtcNow, cancellationToken);

            var userDto = _mapper.Map<UserDetailsDto>(user);

            userDto.IsBanned = isBanned;
            userDto.HasActiveSubscription = hasSubscription;

            return Result<UserDetailsDto>.Success(userDto);
        }
    }
}