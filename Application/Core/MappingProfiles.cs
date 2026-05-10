using Application.BannedUsers.Queries.GetBannedUsers;
using Application.Documents;
using Application.OMDb.DTOs;
using Application.Plans.Queries;
using Application.Scrapers.Queries.GetLatestTorrents;
using Application.Subscriptions;
using Application.Tags;
using Application.Tasks.Queries.GetTasks;
using Application.Users.Commands.Update;
using Application.Users.Queries;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Domain.Models.Scrapers.Rarbg;
using Domain.Models.Scrapers.X1337;
using Domain.Models.Scrapers.Yts;

namespace Application.Core;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        // If you want to skip mapping default DateTime values, you must configure this per property in CreateMap calls.

        CreateMap<Command, User>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.IsConfirmed, opt => opt.Ignore())
            .ForMember(dest => dest.TelegramId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.BannedUser, opt => opt.Ignore())
            .ForMember(dest => dest.Subscriptions, opt => opt.Ignore());

        CreateMap<Users.Commands.ActiveDeactive.Command, User>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.Activate))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));


        CreateMap<User, UserDetailsDto>()
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
            .ForMember(dest => dest.IsBanned, opt => opt.Ignore())
            .ForMember(dest => dest.CurrentActiveSubscription, opt => opt.Ignore());

        CreateMap<User, UserBase>();


        CreateMap<Users.Commands.UpdateUsersByAdmin.Command, User>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Image, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForAllMembers(opt =>
                opt.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<BannedUser, BannedUserDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.User.Name))
            .ForMember(dest => dest.FreeAt,
                opt => opt.MapFrom(src => src.FreeAt.HasValue ? src.FreeAt.ToString() : null));

        CreateMap<Tags.Commands.Create.Command, Tag>();

        CreateMap<OmdbItem, OMDbSummaryDto>()
            .ForMember(d => d.ImdbId, opt => opt.MapFrom(s => s.ImdbId))
            .ForMember(d => d.Title, opt => opt.MapFrom(s => s.Title))
            .ForMember(d => d.Poster, opt => opt.MapFrom(s => s.Poster))
            .ForMember(d => d.ImdbVotes, opt => opt.MapFrom(s => s.ImdbVotes))
            .ForMember(d => d.Type, opt => opt.MapFrom(s => s.Type))
            .ForMember(d => d.Year, opt => opt.MapFrom(s => s.Year))
            .ForMember(d => d.ImdbRating,
                opt => opt.MapFrom(s =>
                    s.ImdbRating.HasValue
                        ? Math.Round((decimal)s.ImdbRating.Value, 1, MidpointRounding.AwayFromZero)
                        : (decimal?)null));

        CreateMap<OmdbItem, OmdbItemDto>()
            .PreserveReferences()
            .ForMember(dest => dest.Seasons, opt => opt.MapFrom(src => src.Seasons))
            .ForMember(dest => dest.Documents, opt => opt.MapFrom(src => src.Documents))
            .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags))
            .ForMember(d => d.ImdbRating,
                opt => opt.MapFrom(s =>
                    s.ImdbRating.HasValue
                        ? Math.Round((decimal)s.ImdbRating.Value, 1, MidpointRounding.AwayFromZero)
                        : (decimal?)null));

        CreateMap<Season, SeasonDto>();

        CreateMap<Subscription, SubscriptionDto>();
        CreateMap<Subscription, UserSubscriptionDto>();
        
        CreateMap<Plan, PlanDto>();
        CreateMap<Plans.Commands.UpdatePlan.Command, Plan>()
            .ForMember(dest => dest.Title, opt => opt.Condition((src, dest) => src.Title != null))
            .ForMember(dest => dest.Length, opt => opt.Condition((src, dest) => src.Length.HasValue))
            .ForMember(dest => dest.Price, opt => opt.Condition((src, dest) => src.Price.HasValue))
            .ForMember(dest => dest.DiscountPercent, opt => opt.Condition((src, dest) => src.DiscountPercent.HasValue))
            .ForMember(dest => dest.DiscountReason, opt => opt.Condition((src, dest) => src.DiscountReason != null))
            .ForMember(dest => dest.NumberOfDownloads,
                opt => opt.Condition((src, dest) => src.NumberOfDownloads.HasValue))
            .ForMember(dest => dest.NumberOfRequest, opt => opt.Condition((src, dest) => src.NumberOfRequest.HasValue))
            .ForMember(dest => dest.IsActive, opt => opt.Condition((src, dest) => src.IsActive.HasValue));

        CreateMap<Document, DocumentDto>();
        CreateMap<Tag, TagDtoNoSummary>();

        CreateMap<Tag, TagDto>()
            .ForMember(d => d.OmdbSummary, opt => opt.MapFrom(s => s.OmdbItem));

        CreateMap<Tasks.Commands.AddTorrentTask.Command, TorrentTask>();

        CreateMap<Plans.Commands.CreatePlan.Command, Plan>();

        CreateMap<RarbgPreview, AbstractedLatestTorrent>()
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.DetailUrl, opt => opt.MapFrom(src => src.TitleHref))
            .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category))
            .ForMember(dest => dest.CategoryUrl, opt => opt.MapFrom(src => src.CategoryHref))
            .ForMember(dest => dest.Size, opt => opt.MapFrom(src => src.Size))
            .ForMember(dest => dest.Seeders, opt => opt.MapFrom(src => src.Seeders))
            .ForMember(dest => dest.Leechers, opt => opt.MapFrom(src => src.Leechers))
            .ForMember(dest => dest.MagnetLink, opt => opt.MapFrom(src => src.MagnetLink))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(_ => TorrentSource.RARBG));

        CreateMap<X1337Preview, AbstractedLatestTorrent>()
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.DetailUrl, opt => opt.MapFrom(src => src.RefLink))
            .ForMember(dest => dest.Size, opt => opt.MapFrom(src => src.FileSize))
            .ForMember(dest => dest.Seeders, opt => opt.MapFrom(src => TryParseInt(src.Seeds)))
            .ForMember(dest => dest.Leechers, opt => opt.MapFrom(src => TryParseInt(src.Leeches)))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(_ => TorrentSource.X1337));

        CreateMap<YtsPreview, AbstractedLatestTorrent>()
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.Year, opt => opt.MapFrom(src => src.Year))
            .ForMember(dest => dest.DetailUrl, opt => opt.MapFrom(src => src.DetailUrl))
            .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImageUrl))
            .ForMember(dest => dest.Rating, opt => opt.MapFrom(src => src.Rating))
            .ForMember(dest => dest.Genres, opt => opt.MapFrom(src => src.Genres))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(_ => TorrentSource.YTS));

        CreateMap<TorrentTask, TaskDto>()
            .ForMember(dest => dest.State, opt => opt.MapFrom(src => src.State.ToString()))
            .ForMember(dest => dest.TaskType, opt => opt.MapFrom(src => src.TaskType.ToString()))
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority.ToString()))
            .ForMember(dest => dest.DownloadProgress, opt => opt.MapFrom(src => src.TaskDownloadProgress.Progress))
            .ForMember(dest => dest.DownloadSize, opt => opt.MapFrom(src => src.TaskDownloadProgress.Size))
            .ForMember(dest => dest.DownloadSpeed, opt => opt.MapFrom(src => src.TaskDownloadProgress.Speed))
            .ForMember(dest => dest.UploadProgress, opt => opt.MapFrom(src => src.TaskUploadProgress));

        CreateMap<TaskUploadProgress, UploadProgressDto>();

        CreateMap<Document, DocumentDto>();
    }

    private static int TryParseInt(string value)
    {
        return int.TryParse(value, out var result) ? result : 0;
    }
}