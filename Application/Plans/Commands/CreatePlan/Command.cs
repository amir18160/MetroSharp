using Application.Core;
using MediatR;

namespace  Application.Plans.Commands.CreatePlan;

public class Command: IRequest<Result<Unit>>
{
    public string Title { get; set; }
    public int Length { get; set; }
    public int Price { get; set; }
    public int? DiscountPercent { get; set; } = 0;
    public string DiscountReason { get; set; }
    public int NumberOfDownloads { get; set; }
    public int NumberOfRequest { get; set; }
}