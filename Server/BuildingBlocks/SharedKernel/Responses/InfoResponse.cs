namespace SharedKernel.Responses;

public record InfoResponse
{
    public string? Message { get; init; }
    public string? Error { get; init; }
    public bool IsSuccess => Error == null;
    
    public static InfoResponse Success(string message) => new() { Message = message };
    public static InfoResponse Failure(string error) => new() { Error = error };
}