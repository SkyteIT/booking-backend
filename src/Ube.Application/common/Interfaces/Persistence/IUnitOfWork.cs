namespace Ube.Application.Common.Interfaces.Persistence;
public interface IUnitOfWork
{
    Task BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();
}