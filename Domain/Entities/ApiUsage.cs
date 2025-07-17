using Domain.Enums;

namespace Domain.Entities
{
    public class ApiUsage
    {
        public string ApiKey { get; set; }
        public DateTime Date { get; set; }
        public int Count { get; set; }
        public ApiServiceType ApiType { get; set; }
    }
}