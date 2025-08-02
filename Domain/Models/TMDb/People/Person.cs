using System;
using System.Collections.Generic;

using Domain.Models.TMDb.Changes;
using Domain.Models.TMDb.General;

namespace Domain.Models.TMDb.People
{
    public class Person
    {
        
        public bool Adult { get; set; }

        
        public List<string> AlsoKnownAs { get; set; }

        
        public string Biography { get; set; }

        public DateTime? Birthday { get; set; }

        
        public ChangesContainer Changes { get; set; }

    
        public DateTime? Deathday { get; set; }

        
        public ExternalIdsPerson ExternalIds { get; set; }

        
        public PersonGender Gender { get; set; }

        
        public string Homepage { get; set; }

        
        public int Id { get; set; }

        
        public ProfileImages Images { get; set; }

        
        public string ImdbId { get; set; }

        
        public MovieCredits MovieCredits { get; set; }

        
        public string Name { get; set; }

        
        public string PlaceOfBirth { get; set; }

        
        public double Popularity { get; set; }

        
        public string KnownForDepartment { get; set; }

        
        public string ProfilePath { get; set; }

        
        public SearchContainer<TaggedImage> TaggedImages { get; set; }

        
        public TvCredits TvCredits { get; set; }

        
        public TranslationsContainer Translations { get; set; }
    }
}