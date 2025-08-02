using System.Collections.Generic;



namespace Domain.Models.TMDb.Search
{
    public class SearchCollection : SearchBase
    {

         public bool Adult { get; set; }
               
        public string BackdropPath { get; set; }

        public string Name = null;
        
        
        public string OriginalLanguage { get; set; }


        public string OriginalName = null;
        
        
        public string Overview { get; set; }

        
        public string PosterPath { get; set; }
    }
}