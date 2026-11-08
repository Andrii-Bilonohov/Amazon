namespace SharedKernel;

public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid(); 

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public bool IsDeleted { get; private set; }

    protected BaseEntity()
    {
        var now = DateTime.UtcNow;
        CreatedAt = now;
        UpdatedAt = now;
    }

    protected void Touch() => UpdatedAt = DateTime.UtcNow;

    protected void Delete()
    {
        if (IsDeleted) return;
        IsDeleted = true;
        Touch();
    }
}