using System;



namespace Domain.Models.TMDb.Changes
{
    public abstract class ChangeItemBase
    {
        
        public ChangeAction Action { get; set; }

        
        public string Id { get; set; }

        
        public string Iso_639_1 { get; set; }

        

        public DateTime Time { get; set; }
    }
}