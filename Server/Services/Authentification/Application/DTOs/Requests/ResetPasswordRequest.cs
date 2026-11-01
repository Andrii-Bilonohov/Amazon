namespace Application.DTOs.Requests;

public record ResetPasswordRequest(string Email, string Code, string NewPassword);