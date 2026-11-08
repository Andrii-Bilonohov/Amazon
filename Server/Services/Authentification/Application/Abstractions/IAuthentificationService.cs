using Application.DTOs.Requests;
using Application.DTOs.Responses;
using SharedKernel.Responses;

namespace Application.Abstractions;

public interface IAuthentificationService
{
    Task<InfoResponse> InitiateRegisterUserAsync(RegisterUserRequest userRequest, CancellationToken cancellationToken = default);
    Task<ItemResponse<AuthUserResponse>> ConfirmEmailAsync(ConfirmEmailRequest confirmEmailRequest, CancellationToken cancellationToken = default);
    Task<ItemResponse<AuthUserResponse>> LoginUserAsync(LoginUserRequest userRequest, CancellationToken cancellationToken = default);
    Task<InfoResponse> InitiatePasswordResetAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default);
    Task<InfoResponse> ConfirmPasswordResetAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default);
}