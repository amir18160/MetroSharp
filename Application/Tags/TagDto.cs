using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.OMDb;
using Domain.Entities;
using Domain.Enums;

namespace Application.Tags
{
    public class TagDto
    {
        public Guid Id { get; set; }
        public TagType Type { get; set; }
        public string Description { get; set; }
        public bool IsPinned { get; set; }
        public DateTime CreatedAt { get; set; }


        public Guid OmdbItemId { get; set; }
        public OMDbSummaryDto OmdbSummary { get; set; }
    }
}