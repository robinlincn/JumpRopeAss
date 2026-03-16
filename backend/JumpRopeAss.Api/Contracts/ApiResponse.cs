namespace JumpRopeAss.Api.Contracts;

public sealed class ApiResponse<T>
{
    public int Code { get; init; }
    public string Message { get; init; } = "OK";
    public T? Data { get; init; }

    public static ApiResponse<T> Ok(T data) => new() { Code = 0, Message = "OK", Data = data };
    public static ApiResponse<T> Fail(int code, string message) => new() { Code = code, Message = message, Data = default };
}

