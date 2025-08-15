using System.Reflection;
using AutoMapper;
using Domain.Models.Qbit;
using Domain.Models.Scrapers.Common;
using Domain.Models.Scrapers.Sub2fm;
using Domain.Models.Scrapers.Subsource;
using Domain.Models.TMDb.General;
using QBittorrent.Client;

namespace Infrastructure.Core
{
    public class MappingProfiles : Profile
    {
        private const string ImageBaseUrl = "https://image.tmdb.org/t/p/original/";
        public MappingProfiles()
        {
            CreateMapsForMatchingTypes();

            CreateMap<TorrentInfo, QbitTorrentInfo>();

            CreateMap<TMDbLib.Objects.Search.SearchMovie, TMDbMedia>()
                .ForMember(dest => dest.MediaType, opt => opt.MapFrom(src => MediaType.Movie))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.OriginalTitle, opt => opt.MapFrom(src => src.OriginalTitle))
                .ForMember(dest => dest.Poster, opt => opt.MapFrom(src => BuildImageUrl(src.PosterPath)))
                .ForMember(dest => dest.Backdrop, opt => opt.MapFrom(src => BuildImageUrl(src.BackdropPath)));

            CreateMap<TMDbLib.Objects.Search.SearchTv, TMDbMedia>()
                .ForMember(dest => dest.MediaType, opt => opt.MapFrom(src => MediaType.Tv))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.OriginalTitle, opt => opt.MapFrom(src => src.OriginalName))
                .ForMember(dest => dest.Poster, opt => opt.MapFrom(src => BuildImageUrl(src.PosterPath)))
                .ForMember(dest => dest.Backdrop, opt => opt.MapFrom(src => BuildImageUrl(src.BackdropPath)));

            CreateMap<TMDbLib.Objects.Movies.Movie, Domain.Models.TMDb.Movies.Movie>();
            CreateMap<TMDbLib.Objects.TvShows.TvShow, Domain.Models.TMDb.TvShows.TvShow>();

            CreateMap<SubsourceSearchResult, SubtitleSearch>()
                .ForMember(dest => dest.Link, opt => opt.MapFrom(src => src.Href));
            CreateMap<SubF2mSubtitleSearchResult, SubtitleSearch>()
                .ForMember(dest => dest.Link, opt => opt.MapFrom(src => src.Url));

            CreateMap<SubF2mSubtitleDetail, SubtitleListItem>()
                .ForMember(dest => dest.Link, opt => opt.MapFrom(src => src.Url))
                .ForMember(dest => dest.Source, opt => opt.MapFrom(src => SubtitleSource.Sub2fm))
                .ForMember(dest => dest.Caption, opt => opt.MapFrom(src => src.Description));       
        }


        private static string BuildImageUrl(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return null;

            var cleanPath = path.TrimStart('/').Trim();

            return $"{ImageBaseUrl}{cleanPath}";
        }

        private void CreateMapsForMatchingTypes()
        {
            var sourceAssembly = typeof(TMDbLib.Objects.Movies.Movie).Assembly;
            var destinationAssembly = typeof(Domain.Models.TMDb.Movies.Movie).Assembly;

            const string sourceNamespace = "TMDbLib.Objects";
            const string destinationNamespace = "Domain.Models.TMDb";

            // Properties to process as image URLs
            var imagePathProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "BackdropPath",
                "ProfilePath",
                "PosterPath"
            };

            var destinationTypes = destinationAssembly.GetExportedTypes()
                .Where(t => t.IsClass &&
                            t.Namespace != null &&
                            t.Namespace.StartsWith(destinationNamespace));

            foreach (var destinationType in destinationTypes)
            {
                var sourceTypeName = destinationType.FullName.Replace(destinationNamespace, sourceNamespace);
                var sourceType = sourceAssembly.GetType(sourceTypeName);

                if (sourceType == null)
                    continue;

                // Create the map
                var map = CreateMap(sourceType, destinationType);

                // Look for destination properties that match our image paths
                foreach (var destProp in destinationType.GetProperties()
                    .Where(p => p.PropertyType == typeof(string) &&
                                imagePathProperties.Contains(p.Name)))
                {
                    var sourceProp = sourceType.GetProperty(destProp.Name,
                        BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                    if (sourceProp == null || sourceProp.PropertyType != typeof(string))
                        continue;

                    // Dynamically set mapping for image properties
                    map.ForMember(destProp.Name, opt => opt.MapFrom((src, dest, destMember, context) =>
                    {
                        var sourceValue = (string)sourceProp.GetValue(src);
                        return BuildImageUrl(sourceValue);
                    }));
                }
            }
        }
    }
}