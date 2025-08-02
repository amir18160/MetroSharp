

namespace Domain.Models.TMDb.Changes
{
    public class ChangeItemDestroyed : ChangeItemBase
    {
        public ChangeItemDestroyed()
        {
            Action = ChangeAction.Destroyed;
        }

        
        public object Value { get; set; }
    }
}