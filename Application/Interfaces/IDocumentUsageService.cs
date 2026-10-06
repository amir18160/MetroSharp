using Application.DocumentUsages.DTOs;

namespace Application.Interfaces;

public interface IDocumentUsageService
{
    public Task<DocumentUsageStats> GetGeneralStats();
    public Task<UserUsage> GetUserUsageStats(string userId);
    public Task<UserRemainingUsage> GetUserRemainingUsage(string userId);
}