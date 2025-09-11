using Application.Core;
using MediatR;

namespace Application.Dashboard.Queries.GetSystemInfo;

public class Query : IRequest<Result<SystemInfoDto>>
{
}