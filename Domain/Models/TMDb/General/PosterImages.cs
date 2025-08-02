using System.Collections.Generic;

namespace Domain.Models.TMDb.General
{
    public class PosterImages
    {

        public int Id { get; set; }

       
        public List<ImageData> Posters { get; set; }
    }
}