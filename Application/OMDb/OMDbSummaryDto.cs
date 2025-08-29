using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Enums;

namespace Application.OMDb
{
    public class OMDbSummaryDto
    {
        public Guid Id { get; set; }
        public string ImdbId { get; set; }
        public string Title { get; set; }
        public string Poster { get; set; }
        public double? ImdbRating { get; set; }
        public double? ImdbVotes { get; set; }
        public OmdbItemType Type { get; set; }
        public int? Year { get; set; }
    
    }
}