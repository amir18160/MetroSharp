using System.Collections.Generic;

namespace Domain.Models.TMDb.General
{
    public class Images
    {

        public List<ImageData> Backdrops { get; set; }

        public List<ImageData> Posters { get; set; }

        public List<ImageData> Logos { get; set; }
    }
}