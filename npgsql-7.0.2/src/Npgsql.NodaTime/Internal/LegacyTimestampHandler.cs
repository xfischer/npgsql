using System;
using System.Diagnostics;
using NodaTime;
using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.Internal;
using EnterpriseDB.EDBClient.Internal.TypeHandling;
using EnterpriseDB.EDBClient.PostgresTypes;
using BclTimestampHandler = EnterpriseDB.EDBClient.Internal.TypeHandlers.DateTimeHandlers.TimestampHandler;

namespace EnterpriseDB.EDBClient.NodaTime.Internal;

sealed partial class LegacyTimestampHandler : EDBSimpleTypeHandler<Instant>,
    IEDBSimpleTypeHandler<LocalDateTime>, IEDBSimpleTypeHandler<DateTime>, IEDBSimpleTypeHandler<long>
{
    readonly BclTimestampHandler _bclHandler;

    internal LegacyTimestampHandler(PostgresType postgresType)
        : base(postgresType)
        => _bclHandler = new BclTimestampHandler(postgresType);

    #region Read

    public override Instant Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription = null)
        => TimestampTzHandler.ReadInstant(buf);

    LocalDateTime IEDBSimpleTypeHandler<LocalDateTime>.Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription)
        => TimestampHandler.ReadLocalDateTime(buf);

    DateTime IEDBSimpleTypeHandler<DateTime>.Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription)
        => _bclHandler.Read(buf, len, fieldDescription);

    long IEDBSimpleTypeHandler<long>.Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription)
        => ((IEDBSimpleTypeHandler<long>)_bclHandler).Read(buf, len, fieldDescription);

    #endregion Read

    #region Write

    public override int ValidateAndGetLength(Instant value, EDBParameter? parameter)
        => 8;

    int IEDBSimpleTypeHandler<LocalDateTime>.ValidateAndGetLength(LocalDateTime value, EDBParameter? parameter)
        => 8;

    public override void Write(Instant value, EDBWriteBuffer buf, EDBParameter? parameter)
        => TimestampTzHandler.WriteInstant(value, buf);

    void IEDBSimpleTypeHandler<LocalDateTime>.Write(LocalDateTime value, EDBWriteBuffer buf, EDBParameter? parameter)
        => TimestampHandler.WriteLocalDateTime(value, buf);

    int IEDBSimpleTypeHandler<DateTime>.ValidateAndGetLength(DateTime value, EDBParameter? parameter)
        => ((IEDBSimpleTypeHandler<DateTime>)_bclHandler).ValidateAndGetLength(value, parameter);

    public int ValidateAndGetLength(long value, EDBParameter? parameter)
        => ((IEDBSimpleTypeHandler<long>)_bclHandler).ValidateAndGetLength(value, parameter);

    void IEDBSimpleTypeHandler<DateTime>.Write(DateTime value, EDBWriteBuffer buf, EDBParameter? parameter)
        => ((IEDBSimpleTypeHandler<DateTime>)_bclHandler).Write(value, buf, parameter);

    void IEDBSimpleTypeHandler<long>.Write(long value, EDBWriteBuffer buf, EDBParameter? parameter)
        => ((IEDBSimpleTypeHandler<long>)_bclHandler).Write(value, buf, parameter);

    #endregion Write
}