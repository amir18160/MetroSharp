

namespace Domain.Models.TMDb.General
{
    public class SingleResultContainer<T>
    {

        public int Id { get; set; }

        public T Results { get; set; }
    }
}