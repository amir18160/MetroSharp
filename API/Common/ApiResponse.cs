using System.Text.Json.Serialization;

namespace API.Common
{
    public class ApiResponse<T>
    {
        public string Status { get; set; }
        public T Data { get; set; }
        public List<string> Messages { get; set; }


        public static ApiResponse<T> Success(T data) => new()
        {
            Status = "success",
            Data = data
        };

        public static ApiResponse<T> Error(params string[] messages) => new()
        {
            Status = "error",
            Messages = messages.ToList()
        };

        public static ApiResponse<T> Error(List<string> messages) => new()
        {
            Status = "error",
            Messages = messages
        };
    }
}
