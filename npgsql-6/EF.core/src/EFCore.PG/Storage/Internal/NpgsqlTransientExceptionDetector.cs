using System;

namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Storage.Internal;

/// <summary>
///     Detects the exceptions caused by PostgreSQL or network transient failures.
/// </summary>
public class NpgsqlTransientExceptionDetector
{
    public static bool ShouldRetryOn(Exception? ex)
        => (ex as EDBException)?.IsTransient == true || ex is TimeoutException;
}