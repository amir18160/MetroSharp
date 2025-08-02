using System.Collections.Generic;


namespace Domain.Models.TMDb.General
{
    public class ConfigImageTypes
    {

        public List<string> BackdropSizes { get; set; }


        public string BaseUrl { get; set; }
        public List<string> LogoSizes { get; set; }


        public List<string> PosterSizes { get; set; }


        public List<string> ProfileSizes { get; set; }

        public string SecureBaseUrl { get; set; }

        public List<string> StillSizes { get; set; }
    }
}