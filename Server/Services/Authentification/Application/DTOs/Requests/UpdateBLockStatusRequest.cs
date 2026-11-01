namespace Application.DTOs.Requests;

public record UpdateBlockStatusRequest
(
    Guid Id,
    bool IsBlocked
);