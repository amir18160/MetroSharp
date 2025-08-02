
using System.Collections.Generic;

namespace Domain.Models.TMDb.General
{
    public class AlternativeNames
    {
        public int Id { get; set; }
        public List<AlternativeName> Results { get; set; }
    }
}