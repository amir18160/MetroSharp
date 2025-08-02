using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Domain.Models.TMDb.General
{
    public class MultiSearchParams
    {
        public string Query { get; set; }
        public int Year { get; set; }
        public int Page { get; set; }
    }
}