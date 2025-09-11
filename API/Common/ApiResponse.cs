namespace API.Common;

public class ApiResponse<T>
{
    public string Status { get; set; }
    public T Data { get; set; }
    public List<string> Messages { get; set; }


    public static ApiResponse<T> Success(T data)
    {
        return new ApiResponse<T>
        {
            Status = "success",
            Data = data
        };
    }

    public static ApiResponse<T> Error(params string[] messages)
    {
        return new ApiResponse<T>
        {
            Status = "error",
            Messages = messages.ToList()
        };
    }

    public static ApiResponse<T> Error(List<string> messages)
    {
        return new ApiResponse<T>
        {
            Status = "error",
            Messages = messages
        };
    }
}