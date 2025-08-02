

namespace Domain.Models.TMDb.People
{
    [Flags]
    public enum PersonMethods
    {

        Undefined = 0,
        MovieCredits = 1,
        TvCredits = 2,

        ExternalIds = 4,

        Images = 8,

        TaggedImages = 16,

        Changes = 32,

        Translations = 64,
    }
}