using System;

namespace Domain.Models.TMDb.Credit
{
    public class CreditSeason
    {
        public DateTime? AirDate { get; set; }

        public string PosterPath { get; set; }

        public int SeasonNumber { get; set; }
    }
}