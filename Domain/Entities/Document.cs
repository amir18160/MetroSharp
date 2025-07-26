using Domain.Enums;

namespace Domain.Entities
{
    public class Document
    {
        public Guid Id { get; set; }
        public string FileName { get; set; }
        public string MimeType { get; set; }
        public string FileId { get; set; }
        public string UniqueFileId { get; set; }
        public long FileSize { get; set; }
        public string ChatName { get; set; }
        public int MessageId { get; set; }
        public long ChatId { get; set; }
        public bool IsSubbed { get; set; }
        public string Resolution { get; set; }
        public string Codec { get; set; }
        public string Encoder { get; set; }
        public DocumentType Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        /**********************/
        /***** Relations ******/
        /**********************/
        public Guid? EpisodeId { get; set; }
        public Episode Episode { get; set; }


        public Guid? OmdbItemId { get; set; }
        public OmdbItem OmdbItem { get; set; }

    }
}