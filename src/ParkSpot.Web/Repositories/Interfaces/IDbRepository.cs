using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using ParkSpot.Web.Database;

namespace ParkSpot.Web.Repositories.Interfaces;

public interface IDbRepository
{
    Task<DbTransaction> BeginTransactionAsync(CancellationToken cancellationToken);
    Task<int> RawSqlExecuteAsync(RawSqlCommand sql, CancellationToken cancellationToken, IDbTransaction? transaction = null);
    Task<IReadOnlyList<T>> RawSqlFetchListAsync<T>(RawSqlCommand sql, CancellationToken cancellationToken, IDbTransaction? transaction = null);
    Task<T> RawSqlFetchRequiredSingleAsync<T>(RawSqlCommand sql, CancellationToken cancellationToken, IDbTransaction? transaction = null);
    Task<T?> RawSqlFetchSingleAsync<T>(RawSqlCommand sql, CancellationToken cancellationToken, IDbTransaction? transaction = null);
    Task<T?> RawSqlExecuteScalarAsync<T>(RawSqlCommand sql, CancellationToken cancellationToken, IDbTransaction? transaction = null);
}
