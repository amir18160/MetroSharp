using System.Collections.Generic;


namespace Domain.Models.TMDb.General
{
    public class TranslationsContainerTv
    {
  
        public int Id { get; set; }

         public List<Translation> Translations { get; set; }
    }
}