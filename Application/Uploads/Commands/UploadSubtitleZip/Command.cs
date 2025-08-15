using Application.Core;
using Application.Uploads.Commands.CommonDTOs;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Uploads.Commands.UploadSubtitleZip
{
    public class Command : IRequest<Result<SubtitleUploadResultDto>>
    {
        public IFormFile File { get; set; }
    }
}