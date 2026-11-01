using Application.DTOs.Requests;
using Application.DTOs.Responses;
using SharedKernel.Responses;

namespace Application.Abstractions;

public interface IAuthentificationService
{
    Task<ItemResponse<AuthUserResponse>> RegisterUser(RegisterUserRequest userRequest, CancellationToken cancellationToken = default);
    Task<ItemResponse<AuthUserResponse>> LoginUser(LoginUserRequest userRequest, CancellationToken cancellationToken = default);
    Task<InfoResponse> InitiatePasswordReset(ForgotPasswordRequest request, CancellationToken cancellationToken = default);
    Task<InfoResponse> CompletePasswordReset(ResetPasswordRequest request, CancellationToken cancellationToken = default);
}