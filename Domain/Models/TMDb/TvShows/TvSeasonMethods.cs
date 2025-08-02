

namespace Domain.Models.TMDb.TvShows
{
    [Flags]
    public enum TvSeasonMethods
    {
    
        Undefined = 0,
       
        Credits = 1,
       
        Images = 2,

        ExternalIds = 4,
     
        Videos = 8,
   
        AccountStates = 16,
    
        Translations = 32,
    }
}
