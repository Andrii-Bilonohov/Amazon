using Domain.Abstractions;
using Domain.Entities;
using Infrastructure.Repositories;

namespace Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly UserContext _context;
    private IBaseRepository<User>? _users;

    public UnitOfWork(UserContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }
    
    public IBaseRepository<User> Users =>
        _users ??= new BaseRepository<User>(_context);
    
    public async Task<int> CommitAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}