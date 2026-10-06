using Application.DocumentUsages.DTOs;
using Application.Interfaces;
using Application.Users.DTOs;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.DocumentUsageService.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Persistence;

namespace Infrastructure.DocumentUsageService;

public class DocumentUsageService: IDocumentUsageService
{
    private readonly DataContext _context;
    private readonly UserManager<User> _userManager;
    private readonly IMapper _mapper;
    private readonly FreePlanSettings  _freePlanSettings;

    public DocumentUsageService(DataContext context, UserManager<User> userManager, IMapper mapper, IOptions<FreePlanSettings> freePlanSettings)
    {
        _freePlanSettings = freePlanSettings.Value;
        _context = context;
        _userManager = userManager;
        _mapper = mapper;
    }
    
    public async Task<DocumentUsageStats> GetGeneralStats()
    {
        var stats = new DocumentUsageStats
        {
            Last24HoursUsage = await GetLastDaysStats(1),
            Last7DaysUsage = await GetLastDaysStats(7),
            Last30DaysUsage = await GetLastDaysStats(30),
            Last90DaysUsage = await GetLastDaysStats(90),
            LastYearUsage = await GetLastDaysStats(365),
            TotalUsages =
                await GetLastDaysStats(365 * 10) // I will probably end my life before this date anyway, cause im a fucking looser. I was born to lose: 5/11/2036
        };

        return stats;
    }

    public async Task<UserUsage> GetUserUsageStats(string userId)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            throw new Exception("User not found");
        }

        var userStats = new UserUsage
        {
            Summaries = new UserUsageSummaries
            {
                TodayUsage = await GetLastDaysStats(1, user),
                Last7Days = await GetLastDaysStats(7, user),
                Last30Days = await GetLastDaysStats(30, user),
            },
            User = _mapper.Map<UserBase>(user),
        };

        return userStats;
    }

    public async Task<UserRemainingUsage> GetUserRemainingUsage(string userId)
    {
        var user = await _context.Users
            .Include(x => x.Subscriptions)
            .ThenInclude(x => x.Plan)
            .Where(x => x.Subscriptions.Any(s =>
                s.UserId == userId && s.EndAt >= DateTime.UtcNow.Date)) // possible error
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            throw new Exception("User not found");
        }

        var linkLimit = _freePlanSettings.NumberOfDirectLinks;
        var fileLimit = _freePlanSettings.NumberOfFileDownloads;
        var requestLimit = _freePlanSettings.NumberOfRequest;

        if (user.Subscriptions.FirstOrDefault()?.Plan != null)
        {
            linkLimit = user.Subscriptions.FirstOrDefault()!.Plan.NumberOfDirectLinks;
            fileLimit = user.Subscriptions.FirstOrDefault()!.Plan.NumberOfFileDownloads;
            requestLimit = user.Subscriptions.FirstOrDefault()!.Plan.NumberOfRequest;
        }

        var requestedLinkCounts = await _context.DocumentUsages.Where(x =>
                x.UserId == user.Id && x.CreatedAt > DateTime.UtcNow && x.CreatedAt < DateTime.UtcNow.AddDays(-1) &&
                x.UsageType == DocumentUsageType.Link)
            .CountAsync();

        var requestedFilesCounts = await _context.DocumentUsages.Where(x =>
                x.UserId == user.Id && x.CreatedAt >= DateTime.UtcNow && x.CreatedAt <= DateTime.UtcNow.AddDays(-1) &&
                x.UsageType == DocumentUsageType.File)
            .CountAsync();

        var requestedMediaCounts = 0;
        if (user.Subscriptions.FirstOrDefault() == null)
        {
            requestedMediaCounts = await _context.MediaRequests.Where(x =>
                    x.CreatorUserId == user.Id  &&
                    x.CreatedAt >= DateTime.UtcNow.AddDays(-30))
                .CountAsync();
        }
        else if (user.Subscriptions.FirstOrDefault() == null)
        {
            requestedMediaCounts = await _context.MediaRequests.Where(x =>
                    x.CreatorUserId == user.Id &&
                    x.CreatedAt >= user.Subscriptions.FirstOrDefault().StartedAt)
                .CountAsync();
        }
        
        var remaining = new UserRemainingUsage
        {
            MaxAllowedLink = linkLimit,
            MaxAllowedFiles = fileLimit,
            MaxAllowedRequest = requestLimit,
            RemainedFileRequest = fileLimit - requestedFilesCounts,
            RemainedLinkRequest = linkLimit - requestedLinkCounts,
            RemainedMediaRequest = requestLimit - requestedMediaCounts,
        };

        return remaining;
    }

    private async Task<UsageSummary> GetLastDaysStats(int days, User user = null)
    {
        var summary = new UsageSummary
        {
            File = 0,
            Link = 0,
            Requests = 0,
        };

        var lowDate = DateTime.Today.AddDays(-days).Date;


        var linkUsages = await _context.DocumentUsages
            .Where(x => x.CreatedAt.Date >= lowDate && x.CreatedAt.Date <= DateTime.UtcNow.Date &&
                        x.UsageType == DocumentUsageType.Link && (user == null || user.Id == x.UserId))
            .CountAsync();

        summary.Link = linkUsages;

        var fileUsages = await _context.DocumentUsages
            .Where(x => x.CreatedAt.Date >= lowDate && x.CreatedAt.Date <= DateTime.UtcNow.Date &&
                        x.UsageType == DocumentUsageType.File && (user == null || user.Id == x.UserId))
            .CountAsync();

        summary.File = fileUsages;

        var mediaRequestUsage = await _context.MediaRequests
            .Where(x => x.CreatedAt.Date >= lowDate && x.CreatedAt.Date <= DateTime.UtcNow.Date &&
                        (user == null || user.Id == x.CreatorUserId))
            .CountAsync();

        summary.Requests = mediaRequestUsage;

        return summary;
    }
    
}