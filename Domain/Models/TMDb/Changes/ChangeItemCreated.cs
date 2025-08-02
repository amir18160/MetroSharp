namespace Domain.Models.TMDb.Changes
{
    public class ChangeItemCreated : ChangeItemBase
    {
        public ChangeItemCreated()
        {
            Action = ChangeAction.Created;
        }
    }
}