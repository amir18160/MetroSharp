using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Domain.Models.TMDb.General
{
    public class GetMediaDetailsParams
    {
        public int Id { get; set; }
        public bool IncludeExternalIds { get; set; }
        public bool IncludeImages { get; set; }
        public bool IncludeCredits { get; set; }
        public bool IncludeEpisodeGroups { get; set; }
        public bool IncludeSimilar { get; set; }
    }
}