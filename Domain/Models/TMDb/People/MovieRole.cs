using System;


namespace Domain.Models.TMDb.People
{
    public class MovieRole
    {
        
        public bool Adult { get; set; }

        
        public string Character { get; set; }

        
        public string CreditId { get; set; }

        
        public int Id { get; set; }

        
        public string OriginalTitle { get; set; }

        
        public string PosterPath { get; set; }

        
        public DateTime? ReleaseDate { get; set; }

        
        public string Title { get; set; }
    }
}