using Application.Dashboard.Queries.GetSystemInfo;

namespace Application.Interfaces
{
    public interface ISystemInfoService
    {
        Task<SystemInfoDto> GetSystemInfoAsync();
    }
}
