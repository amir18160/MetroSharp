using Application.Core;
using Domain.Models.Qbit;
using MediatR;

namespace Application.TorrentManager.Queries.GetAllQbitTorrents
{
    public class Query: IRequest<Result<List<QbitTorrentInfo>>>
    {
        
    }
}