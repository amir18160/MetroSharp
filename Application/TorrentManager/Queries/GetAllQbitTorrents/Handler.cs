using Application.Core;
using Application.Interfaces;
using Domain.Models.Qbit;
using MediatR;

namespace Application.TorrentManager.Queries.GetAllQbitTorrents
{
    public class Handler: IRequestHandler<Query, Result<List<QbitTorrentInfo>>>
    {
        private readonly IQbitClient _qbitClient;

        public Handler(IQbitClient qbitClient)
        {
            _qbitClient = qbitClient;
        }

        public async Task<Result<List<QbitTorrentInfo>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var torrents = await _qbitClient.GetAllQbitTorrentsAsync();
            return Result<List<QbitTorrentInfo>>.Success(torrents);
        }
    }
}