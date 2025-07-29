using Application.Core;
using Application.Interfaces;
using MediatR;
namespace Application.Dashboard.Queries.GetSystemInfo
{
    public class Query : IRequest<Result<SystemInfoDto>>
    {
    }
}
