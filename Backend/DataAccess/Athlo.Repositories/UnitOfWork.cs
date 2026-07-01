using Athlo.Database.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Athlo.Repositories;

public class UnitOfWork(AthloDbContext context) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        context.SaveChangesAsync(ct);

    public async Task<IUnitOfWorkTransaction> BeginTransactionAsync(CancellationToken ct = default)
    {
        if (!context.Database.IsRelational())
            return new NoOpUnitOfWorkTransaction();

        var transaction = await context.Database.BeginTransactionAsync(ct);
        return new UnitOfWorkTransaction(transaction);
    }

    private sealed class NoOpUnitOfWorkTransaction : IUnitOfWorkTransaction
    {
        public Task CommitAsync(CancellationToken ct = default) => Task.CompletedTask;
        public Task RollbackAsync(CancellationToken ct = default) => Task.CompletedTask;
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }

    private sealed class UnitOfWorkTransaction(IDbContextTransaction transaction) : IUnitOfWorkTransaction
    {
        public Task CommitAsync(CancellationToken ct = default) =>
            transaction.CommitAsync(ct);

        public Task RollbackAsync(CancellationToken ct = default) =>
            transaction.RollbackAsync(ct);

        public ValueTask DisposeAsync() =>
            transaction.DisposeAsync();
    }
}
