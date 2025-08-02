using System;

namespace Domain.Models.TMDb.Movies
{
    public class Country
    {
        public string Certification { get; set; }       
        public string Iso_3166_1 { get; set; }      
        public bool Primary { get; set; }       
        public DateTime? ReleaseDate { get; set; }
    }
}
