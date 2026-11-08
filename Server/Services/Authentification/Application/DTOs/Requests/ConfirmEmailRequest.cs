namespace Application.DTOs.Requests;

public record ConfirmEmailRequest
(
    string Email,
    string Code
);