using Microsoft.EntityFrameworkCore.Storage;
using Ube.Application.Common.Interfaces.Persistence;

namespace Ube.Infrastructure.Persistence;
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _db;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task BeginTransactionAsync()
    {
        if (_transaction == null)
        {
            _transaction = await _db.Database.BeginTransactionAsync();
        }
    }

    public async Task CommitAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            _transaction = null;
        }
    }

    public async Task RollbackAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            _transaction = null;
        }
    }
}