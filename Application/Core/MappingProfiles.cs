using AutoMapper;
using Domain.Entities;

namespace Application.Core
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<Users.Commands.Update.Command, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.IsConfirmed, opt => opt.Ignore())
                .ForMember(dest => dest.TelegramId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.BannedUser, opt => opt.Ignore())
                .ForMember(dest => dest.Subscriptions, opt => opt.Ignore());

            CreateMap<Users.Commands.ActiveDeactive.Command, User>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.Activate))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Username));


            CreateMap<User, Users.Queries.GetSingleUser.UserDetailsDto>()
             .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt.ToString("O"))) // convert DateTime to ISO string
             .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt.ToString("O")))
             .ForMember(dest => dest.IsBanned, opt => opt.Ignore())
             .ForMember(dest => dest.HasActiveSubscription, opt => opt.Ignore());


            CreateMap<Users.Commands.UpdateUsersByAdmin.Command, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Image, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForAllMembers(opt =>
                    opt.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<BannedUser, BannedUsers.Queries.GetBannedUsers.BannedUserDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.User.Name))
                .ForMember(dest => dest.FreeAt, opt => opt.MapFrom(src => src.FreeAt.HasValue ? src.FreeAt.ToString() : null));
        }
    }
}
