

namespace Domain.Models.TMDb.Reviews
{
    public class ReviewBase
    {
        
        public string Author { get; set; }

        
        public AuthorDetails AuthorDetails { get; set; }

        
        public string Content { get; set; }

        
        public string Id { get; set; }

        
        public string Url { get; set; }

        
        public DateTime CreatedAt { get; set; }

        
        public DateTime UpdatedAt { get; set; }
    }
}