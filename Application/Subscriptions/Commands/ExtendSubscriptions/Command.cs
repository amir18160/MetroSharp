using Application.Core;
using Domain.Entities;
using MediatR;

namespace Application.Subscriptions.Commands.ExtendSubscriptions;

public class Command : IRequest<Result<Unit>>
{
    public int ExpirationMargin { get; set; }
    public int DaysToAdd { get; set; }
}