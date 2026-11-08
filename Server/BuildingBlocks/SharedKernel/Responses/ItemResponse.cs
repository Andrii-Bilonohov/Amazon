namespace SharedKernel.Responses;

public record ItemResponse<T>
{
    public T? Data { get; init; }
    public string? Error { get; init; }
    public bool IsSuccess => Error == null;

    public static ItemResponse<T> Success(T? data) => new() { Data = data };
    public static ItemResponse<T> Failure(string error) => new() { Error = error };
}