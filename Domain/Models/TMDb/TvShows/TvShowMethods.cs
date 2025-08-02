using System;


namespace Domain.Models.TMDb.TvShows
{
    [Flags]
    public enum TvShowMethods
    {
    
        Undefined = 0,
     
        Credits = 1 << 0,
       
        Images = 1 << 1,
       
        ExternalIds = 1 << 2,
       
        ContentRatings = 1 << 3,
      
        AlternativeTitles = 1 << 4,
        
        Keywords = 1 << 5,
      
        Similar = 1 << 6,
  
        Videos = 1 << 7,
     
        Translations = 1 << 8,
     
        AccountStates = 1 << 9,
       
        Changes = 1 << 10,
      
        Recommendations = 1 << 11,
  
        Reviews = 1 << 12,
     
        WatchProviders = 1 << 13,
    
        EpisodeGroups = 1 << 14,
     
        CreditsAggregate = 1 << 15,
    }
}
