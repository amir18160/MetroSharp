using Application.Core;
using Application.Uploads.Commands.CommonDTOs;
using MediatR;

namespace Application.Uploads.Commands.UploadFromSubtitleLink
{
    public class Command : IRequest<Result<SubtitleUploadResultDto>>
    {
        public string URL { get; set; }
        public string FileName { get; set; }
    }
}