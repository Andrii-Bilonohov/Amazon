using Application.Abstractions;
using Application.DTOs.Requests;
using Domain.Abstractions;
using Microsoft.Extensions.Logging;
using SharedKernel.Responses;

namespace Application.Services;

public class UsersService : IUsersService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UsersService> _logger;

    public UsersService(IUnitOfWork unitOfWork, ILogger<UsersService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<InfoResponse> UpdateBLockStatusAsync(
        UpdateBlockStatusRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(request.Id, cancellationToken);
            if (user == null)
                return InfoResponse.Failure("User not found");
            
            user.UpdateBlockStatus(request.IsBlocked);
            _unitOfWork.Users.Update(user);
            await _unitOfWork.CommitAsync(cancellationToken);

            return InfoResponse.Success(
                $"User with id {user.Id} successfully {(request.IsBlocked ? "blocked" : "unblocked")}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while updating block status");
            return InfoResponse.Failure("Failed to update block status");
        }
    }
    
    public async Task<InfoResponse> UpdateProfileAsync(
        Guid userId, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
            if (user == null)
                return InfoResponse.Failure("User not found");

            if (request.IsEmpty)
                return InfoResponse.Failure("At least one field must be provided.");

            user.UpdateProfile(request.FirstName, request.LastName, request.PhoneNumber,
                request.BirthDate, request.AvatarUrl);
            
            _unitOfWork.Users.Update(user);
            await _unitOfWork.CommitAsync(cancellationToken);
            return InfoResponse.Success($"User with id {user.Id} successfully updated");
        }
        catch (ArgumentException ex)
        {
            return InfoResponse.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while updating profile");
            return InfoResponse.Failure("Failed to update profile");
        }
    }

    public async Task<InfoResponse> SoftDeleteAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
            if (user == null)
                return InfoResponse.Failure("User not found");
            
            user.MarkDeleted();
            _unitOfWork.Users.Update(user);
            await _unitOfWork.CommitAsync(cancellationToken);

            return InfoResponse.Success($"User with id {user.Id} successfully deleted");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while deleting user");
            return InfoResponse.Failure("Failed to delete profile");
        }
    }
}