using Application.Abstractions;
using Application.DTOs.Requests;
using Domain.Abstractions;
using EventBus.UsersEvents;
using Microsoft.Extensions.Logging;
using SharedKernel.Responses;

namespace Application.Services;

public class UsersService : IUsersService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<UsersService> _logger;
    
    public UsersService(IUnitOfWork unitOfWork, IEventPublisher eventPublisher, ILogger<UsersService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
        _logger =  logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<InfoResponse> UpdateBLockStatusAsync(UpdateBlockStatusRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(request.Id, cancellationToken);

            if (user == null)
            {
                return InfoResponse.Failure("User not found");
            }
        
            user.UpdateBlockStatus(request.IsBlocked);

            await _eventPublisher.PublishAsync(new UserChangedBlockStatus(user.Id, user.IsBlocked, DateTime.UtcNow), cancellationToken);
            
            _unitOfWork.Users.Update(user);
            await _unitOfWork.CommitAsync(cancellationToken);
        
            return InfoResponse.Success($"User with id {user.Id} successfully {(request.IsBlocked ? "blocked" : "unblocked")}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while updating block status");
            return InfoResponse.Failure("Failed to update block status");
        }
    }

    public async Task<InfoResponse> UpdateProfileAsync(Guid userId, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var existingUser = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);

            if (existingUser == null)
            {
                return InfoResponse.Failure("User not found");
            }
            
            if (request.IsEmpty)
            {
                return InfoResponse.Failure("At least one field must be provided.");
            }

            existingUser.UpdateProfile(request.FirstName,
                request.LastName,
                request.PhoneNumber,
                request.BirthDate,
                request.AvatarUrl);
            
            await _unitOfWork.CommitAsync(cancellationToken);
            return InfoResponse.Success($"User with id {existingUser.Id} successfully updated");
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
            {
                return InfoResponse.Failure("User not found");
            }

            user.Delete();

            await _eventPublisher.PublishAsync(new UserDeleted(user.Id, user.IsDeleted, DateTime.UtcNow),
                cancellationToken);

            _unitOfWork.Users.Update(user);
            await _unitOfWork.CommitAsync(cancellationToken);

            return InfoResponse.Success($"User with id {user.Id} successfully deleted");
        }
        catch (ArgumentException ex)
        {
            return InfoResponse.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while updating profile");
            return InfoResponse.Failure("Failed to delete profile");
        }
    }
}