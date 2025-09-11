using Application.Core;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Tasks.Commands.AddTorrentTask;

public class Handler : IRequestHandler<Command, Result<Unit>>
{
    private readonly DataContext _context;
    private readonly DownloadContext _downloadContext;
    private readonly ILogger<Handler> _logger;
    private readonly IMapper _mapper;
    private readonly IUserAccessor _userAccessor;
    private readonly IUtilitiesFacade _utilities;

    public Handler(DownloadContext downloadContext, DataContext context, ILogger<Handler> logger,
        IUserAccessor userAccessor, IUtilitiesFacade utilities, IMapper mapper)
    {
        _mapper = mapper;
        _utilities = utilities;
        _userAccessor = userAccessor;
        _logger = logger;
        _context = context;
        _downloadContext = downloadContext;
    }

    public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
    {
        var alreadyExist = await _downloadContext.TorrentTasks
            .FirstOrDefaultAsync(x => _utilities.GetMagnetHash(request.Magnet) == x.TorrentHash);

        if (alreadyExist != null) return Result<Unit>.Failure("A magnet with with hash already exist");

        var user = await _context.Users.FirstOrDefaultAsync(x => x.UserName == _userAccessor.GetUserName(),
            cancellationToken);

        var newTask = _mapper.Map<TorrentTask>(request);
        newTask.TorrentHash = _utilities.GetMagnetHash(request.Magnet);
        newTask.State = TorrentTaskState.Pending;
        newTask.UserId = user?.Id ?? null;
        var addedTask = _downloadContext.Add((object)newTask);
        var res = await _downloadContext.SaveChangesAsync() > 0;

        if (res) return Result<Unit>.Success(Unit.Value);
        return Result<Unit>.Failure("Failed to add new data to database.");
    }
}