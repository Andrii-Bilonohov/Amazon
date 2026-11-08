namespace Application.DTOs.Requests;

public record UpdateUserRequest
(
    string? FirstName, 
    string? LastName, 
    string? PhoneNumber, 
    DateOnly? BirthDate, 
    string? AvatarUrl
)
{
    public bool IsEmpty =>
        string.IsNullOrWhiteSpace(FirstName)
        && string.IsNullOrWhiteSpace(LastName)
        && string.IsNullOrWhiteSpace(AvatarUrl)
        && string.IsNullOrWhiteSpace(PhoneNumber)
        && BirthDate is null;
}