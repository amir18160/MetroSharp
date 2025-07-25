using Application.Core;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.TorrentManager.Commands.AddTorrentProcess
{
    public class Handler : IRequestHandler<Command, Result<Unit>>
    {
        private readonly DownloadContext _downloadContext;
        private readonly DataContext _context;
        private readonly ILogger<Handler> _logger;
        private readonly IUserAccessor _userAccessor;
        private readonly IUtilitiesFacade _utilities;
        private readonly IMapper _mapper;
        public Handler(DownloadContext downloadContext, DataContext context, ILogger<Handler> logger, IUserAccessor userAccessor, IUtilitiesFacade utilities, IMapper mapper)
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

            if (alreadyExist != null)
            {
                return Result<Unit>.Failure("A magnet with with hash already exist");
            }

            var user = await _context.Users.FirstOrDefaultAsync(x => x.UserName == _userAccessor.GetUserName(), cancellationToken: cancellationToken);

            var newTask = _mapper.Map<TorrentTask>(request);
            newTask.TorrentHash = _utilities.GetMagnetHash(request.Magnet);
            newTask.State = Domain.Enums.TorrentTaskState.Pending;
            newTask.UserId = user?.Id ?? null;
            var addedTask = _downloadContext.Add(newTask);
            var res = await _downloadContext.SaveChangesAsync() > 0;

            if (res)
            {
                return Result<Unit>.Success(Unit.Value);
            }
            return Result<Unit>.Failure("Failed to add new data to database.");
        }
    }
}