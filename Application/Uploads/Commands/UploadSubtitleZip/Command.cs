using Application.Core;
using Application.Uploads.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Uploads.Commands.UploadSubtitleZip;

public class Command : IRequest<Result<SubtitleUploadResultDto>>
{
    public IFormFile File { get; set; }
}