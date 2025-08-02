using System;

namespace Domain.Models.TMDb.Movies
{
    [Flags]
    public enum MovieMethods
    {

        Undefined = 0,

        AlternativeTitles = 1 << 0,

        Credits = 1 << 1,

        Images = 1 << 2,

        Keywords = 1 << 3,

        Releases = 1 << 4,

        Videos = 1 << 5,

        Translations = 1 << 6,

        Similar = 1 << 7,

        Reviews = 1 << 8,

        Lists = 1 << 9,

        Changes = 1 << 10,


        AccountStates = 1 << 11,

        ReleaseDates = 1 << 12,

        Recommendations = 1 << 13,

        ExternalIds = 1 << 14,
        WatchProviders = 1 << 15
    }
}
