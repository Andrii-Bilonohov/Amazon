using Application.DTOs.Requests;
using SharedKernel.Responses;

namespace Application.Abstractions;

public interface IUsersService
{
    Task<InfoResponse> UpdateBLockStatusAsync(UpdateBlockStatusRequest request, CancellationToken cancellationToken = default);
    Task<InfoResponse> UpdateProfileAsync(Guid userId, UpdateUserRequest request, CancellationToken cancellationToken = default);
    Task<InfoResponse> SoftDeleteAsync(Guid userId, CancellationToken cancellationToken = default);
}