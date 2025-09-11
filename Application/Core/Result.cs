namespace Application.Core;

public class Result<T>
{
    public bool IsSuccess { get; set; }
    public T Value { set; get; }
    public string Error { set; get; }

    public static Result<T> Success(T Value)
    {
        return new Result<T> { IsSuccess = true, Value = Value };
    }

    public static Result<T> Failure(string Error)
    {
        return new Result<T> { IsSuccess = false, Error = Error };
    }
}