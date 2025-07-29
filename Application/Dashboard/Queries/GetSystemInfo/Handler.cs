using Application.Core;
using Application.Interfaces;
using MediatR;

namespace Application.Dashboard.Queries.GetSystemInfo
{
    public class Handler : IRequestHandler<Query, Result<SystemInfoDto>>
    {
        private readonly ISystemInfoService _systemInfoService;

        public Handler(ISystemInfoService systemInfoService)
        {
            _systemInfoService = systemInfoService;
        }

        public async Task<Result<SystemInfoDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var systemInfo = await _systemInfoService.GetSystemInfoAsync();
            return Result<SystemInfoDto>.Success(systemInfo);
        }
    }
}