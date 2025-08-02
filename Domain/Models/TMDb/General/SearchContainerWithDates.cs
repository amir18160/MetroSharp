
namespace Domain.Models.TMDb.General
{
    public class SearchContainerWithDates<T> : SearchContainer<T>
    {

        public DateRange Dates { get; set; }
    }
}