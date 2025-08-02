

namespace Domain.Models.TMDb.Changes
{
    public class ChangeItemUpdated : ChangeItemBase
    {
        public ChangeItemUpdated()
        {
            Action = ChangeAction.Updated;
        }

        
        public object OriginalValue { get; set; }

        
        public object Value { get; set; }
    }
}