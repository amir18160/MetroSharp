using Application.Core;
using MediatR;

namespace Application.Users.Queries.GetSingleUser
{
    public class Query : IRequest<Result<UserDetailsDto>>
    {
        public string Id { get; set; }
        public string Phone { get; set; }
        public string TelegramId { get; set; }
    }
}