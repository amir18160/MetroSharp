using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Domain.Models.Scrapers.Subsource
{
    public class SubsourceSearchResultResponse
    {
        public bool Success { get; set; }
        public List<SubsourceSearchApiResultItem> Results { get; set; }
    }
}