

namespace Domain.Models.TMDb.General
{
    public class Translation
    {
   
        public string EnglishName { get; set; }

        public string Iso_639_1 { get; set; }

    
        public string Iso_3166_1 { get; set; }

        public string Name { get; set; }


        public TranslationData Data { get; set; }
    }
}