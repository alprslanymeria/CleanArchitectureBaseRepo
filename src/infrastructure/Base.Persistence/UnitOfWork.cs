using Base.Application.Contracts.Persistence;

namespace Base.Persistence;

public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    private readonly AppDbContext _context = context;

    public Task<int> CommitAsync() => _context.SaveChangesAsync();
}