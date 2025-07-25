using Application.Core;
using Domain.Enums;
using MediatR;

namespace Application.TorrentManager.Commands.AddTorrentProcess
{
    public class Command : IRequest<Result<Unit>>
    {
        public string Title { get; set; }
        public TorrentTaskState State { get; set; }
        public string Magnet { get; set; }
        public TorrentTaskType TaskType { get; set; }
        public TorrentTaskPriority Priority { get; set; }
        public string SubtitleUrl { get; set; }
        public string ImdbId { get; set; }
        public int SeasonNumber { get; set; }
        public int EpisodeNumber { get; set; }
    }
}