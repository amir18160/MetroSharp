

namespace Domain.Models.TMDb.Changes
{
    public class ChangeItemAdded : ChangeItemBase
    {
        public ChangeItemAdded()
        {
            Action = ChangeAction.Added;
        }

        
        public object Value { get; set; }
    }
}