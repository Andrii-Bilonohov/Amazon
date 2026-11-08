using Domain.Entities;

namespace Domain.Abstractions;

public interface IUnitOfWork
{
    IBaseRepository<User> Users { get; }
    Task<int> CommitAsync(CancellationToken cancellationToken = default);
}