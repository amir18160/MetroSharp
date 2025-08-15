using Application.Core;
using Application.Interfaces;
using Application.Uploads.Commands.CommonDTOs;
using Domain.Enums;
using Domain.Models.Scrapers.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Uploads.Commands.UploadFromSubtitleLink
{
    public class Handler : IRequestHandler<Command, Result<SubtitleUploadResultDto>>
    {
        private readonly IFileStorageService _storageService;
        private readonly ILogger<Handler> _logger;
        private readonly IScraperFacade _scraperFacade;
        public Handler(IFileStorageService storageService, ILogger<Handler> logger, IScraperFacade scraperFacade)
        {
            _scraperFacade = scraperFacade;
            _logger = logger;
            _storageService = storageService;
        }

        public async Task<Result<SubtitleUploadResultDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                string downloadLink;
                if (request.URL.StartsWith("https://api.subsource.net"))
                {
                    downloadLink = await _scraperFacade.GetSubsourceDownloadLinkAsync(request.URL);
                }
                else if (request.URL.StartsWith("https://subf2m.co"))
                {
                    downloadLink = await _scraperFacade.GetSubF2mDownloadLinkAsync(request.URL);
                }
                else
                {
                    return Result<SubtitleUploadResultDto>.Failure("Provided link is not a supported provider.");
                }

                var path = await _storageService.DownloadAndSaveSubtitle(downloadLink, request.FileName, DownloaderType.Browser);
                return Result<SubtitleUploadResultDto>.Success(
                    new SubtitleUploadResultDto
                    {
                        FilePath = path,
                        FileName = Path.GetFileName(path)
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to download and store subtitle for url: {url} \nError: {error}", request.URL, ex.Message);
                return Result<SubtitleUploadResultDto>.Failure("Failed to download and store subtitle.");
            }
        }
    }
}