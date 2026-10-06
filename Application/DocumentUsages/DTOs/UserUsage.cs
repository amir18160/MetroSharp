using Application.Users.DTOs;
using Application.Users.Queries;

namespace Application.DocumentUsages.DTOs;

public class UserUsage
{
    public UserBase User { get; set; }
    public UserUsageSummaries Summaries { get; set; }
}