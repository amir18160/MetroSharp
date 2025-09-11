namespace Application.Core;

public class AppException
{
    public AppException(int statusCore, string message, string details = null)
    {
        Details = details;
        Message = message;
        StatusCore = statusCore;
    }

    public int StatusCore { get; set; }
    public string Message { get; set; }
    public string Details { get; set; }
}