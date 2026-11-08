using Domain.Enums;

namespace Application.DTOs.Requests;

public record RegisterUserRequest
(
    string Email,
    string Password,
    Role Role
);