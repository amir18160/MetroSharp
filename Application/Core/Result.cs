namespace Application.Core
{
    public class Result<T>
    {
        public bool IsSuccess { get; set; }
        public T Value { set; get; }
        public string Error { set; get; }

        public static Result<T> Success(T Value) => new() { IsSuccess = true, Value = Value };
        public static Result<T> Failure(string Error) => new() { IsSuccess = false, Error = Error };
    }
}