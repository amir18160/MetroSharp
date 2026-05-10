using Application.Core;
using Application.Subscriptions;
using Application.Subscriptions.Queries;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Users.Queries.GetSingleUser;

public class Handler : IRequestHandler<Query, Result<UserDetailsDto>>
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;
    private readonly UserManager<User> _userManager;

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
            user = await _context.Users.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        else if (!string.IsNullOrWhiteSpace(request.TelegramId))
            user = await _context.Users.FirstOrDefaultAsync(x => x.TelegramId == request.TelegramId, cancellationToken);
        else if (!string.IsNullOrWhiteSpace(request.Phone))
            user = await _context.Users.FirstOrDefaultAsync(x => x.PhoneNumber == request.Phone, cancellationToken);

        if (user == null) return null;

        var isBanned = await _context.BannedUsers
            .AnyAsync(x => x.UserId == user.Id && (x.FreeAt == null || x.FreeAt > DateTime.UtcNow), cancellationToken);

        var subscriptions = await _context.Subscriptions
            .Where(x => x.UserId == user.Id && x.EndAt > DateTime.UtcNow)
            .ProjectTo<UserSubscriptionDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
        
        var targetRoles = await _userManager.GetRolesAsync(user);

        var userDto = _mapper.Map<UserDetailsDto>(user);

        userDto.IsBanned = isBanned;
        userDto.CurrentActiveSubscription = subscriptions.FirstOrDefault();
        userDto.Role = targetRoles.FirstOrDefault();

        return Result<UserDetailsDto>.Success(userDto);
    }
}