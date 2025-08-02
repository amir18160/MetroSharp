namespace Domain.Models.TMDb.General
{
    public class SearchContainerWithId<T> : SearchContainer<T>
    {
        public int Id { get; set; }
    }
}