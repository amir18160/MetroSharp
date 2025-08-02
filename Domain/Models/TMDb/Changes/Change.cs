using System.Collections.Generic;


namespace Domain.Models.TMDb.Changes
{
    public class Change
    {
        
        public List<ChangeItemBase> Items { get; set; }

        
        public string Key { get; set; }
    }
}