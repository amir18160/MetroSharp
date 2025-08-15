using Application.Core;
using Application.Interfaces;
using Application.Uploads.Commands.CommonDTOs;
using Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Uploads.Commands.UploadSubtitleZip
{
    public class Handler : IRequestHandler<Command, Result<SubtitleUploadResultDto>>
    {
        private readonly IFileStorageService _storageService;
        private readonly ILogger<Handler> _logger;
        public Handler(IFileStorageService storageService, ILogger<Handler> logger)
        {
            _logger = logger;
            _storageService = storageService;
        }

        public async Task<Result<SubtitleUploadResultDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                var filePath = await _storageService.SaveFileAsync(request.File, FileStorageType.Subtitle);
                var result = new SubtitleUploadResultDto
                {
                    FileName = request.File.FileName,
                    FilePath = filePath
                };
                return Result<SubtitleUploadResultDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save file. Message: {message}", ex.Message);
                return Result<SubtitleUploadResultDto>.Failure($"Failed to save file. Message: {ex.Message}");
            }
        }
    }
}