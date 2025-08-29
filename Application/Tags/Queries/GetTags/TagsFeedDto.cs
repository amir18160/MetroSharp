using Domain.Entities;

namespace Application.Tags.Queries.GetTags
{
    public class TagsFeedDto
    {
        public List<TagDto> Pinned { get; set; }
        public List<TagDto> Recommended { get; set; }
        public List<TagDto> New { get; set; }
        public List<TagDto> NewTopRated { get; set; }
        public List<TagDto> UpdatedSeries { get; set; }
        public List<TagDto> UpdatedMovies { get; set; }
    }
}