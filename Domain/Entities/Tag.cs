using Domain.Enums;

namespace Domain.Entities
{
    public class Tag
    {
        public Guid Id { get; set; }
        public TagType Type { get; set; }
        public string Description { get; set; }
        public bool IsPinned { get; set; }
        public DateTime CreatedAt { get; set; }


        public Guid OmdbItemId { get; set; }
        public OmdbItem OmdbItem { get; set; }
    }
}