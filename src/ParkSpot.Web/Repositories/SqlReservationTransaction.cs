using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using ParkSpot.Web.Repositories.Interfaces;

namespace ParkSpot.Web.Repositories;

public sealed class SqlReservationTransaction : IReservationTransaction
{
    private readonly DbTransaction _transaction;
    private readonly Action _onDisposed;

    public SqlReservationTransaction(DbTransaction transaction, Action onDisposed)
    {
        _transaction = transaction;
        _onDisposed = onDisposed;
    }

    public async Task CommitAsync(CancellationToken cancellationToken)
    {
        await _transaction.CommitAsync(cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            await _transaction.DisposeAsync();
        }
        finally
        {
            _onDisposed();
        }
    }
}
