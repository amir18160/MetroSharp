

namespace Domain.Models.TMDb.Changes
{
    public class ChangeItemDeleted : ChangeItemBase
    {
        public ChangeItemDeleted()
        {
            Action = ChangeAction.Deleted;
        }

        
        public object OriginalValue { get; set; }
    }
}