using Domain.Enums;

namespace Application.Documents
{
    public class DocumentDto
    {
        public Guid Id { get; set; }
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public bool IsSubbed { get; set; }
        public string Resolution { get; set; }
        public string Codec { get; set; }
        public string Encoder { get; set; }
        public DocumentType Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}