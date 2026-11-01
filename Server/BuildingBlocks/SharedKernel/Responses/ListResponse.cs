namespace SharedKernel.Responses;

public record ListResponse<T>
{
    public List<T>? Data { get; init; }
    public string? Error { get; init; }
    public bool IsSuccess => Error == null;
    
    public static ListResponse<T> Success(List<T> data) => new() { Data = data };
    public static ListResponse<T> Failure(string error) => new() { Error = error };
}