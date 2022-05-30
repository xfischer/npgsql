using System;
using System.Threading;
using System.Threading.Tasks;
using NodaTime;
using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.Internal;
using EnterpriseDB.EDBClient.Internal.TypeHandlers;
using EnterpriseDB.EDBClient.Internal.TypeHandling;
using EnterpriseDB.EDBClient.PostgresTypes;
using EDBTypes;

namespace EnterpriseDB.EDBClient.NodaTime.Internal
{
    public partial class DateRangeHandler : RangeHandler<LocalDate>, IEDBTypeHandler<DateInterval>
#if NET6_0_OR_GREATER
        , IEDBTypeHandler<EDBRange<DateOnly>>
#endif
    {
        public DateRangeHandler(PostgresType rangePostgresType, EDBTypeHandler subtypeHandler)
            : base(rangePostgresType, subtypeHandler)
        {
        }

        public override Type GetFieldType(FieldDescription? fieldDescription = null) => typeof(DateInterval);
        public override Type GetProviderSpecificFieldType(FieldDescription? fieldDescription = null) => typeof(DateInterval);

        public override async ValueTask<object> ReadAsObject(EDBReadBuffer buf, int len, bool async,
            FieldDescription? fieldDescription = null)
            => (await Read<DateInterval>(buf, len, async, fieldDescription))!;

        async ValueTask<DateInterval> IEDBTypeHandler<DateInterval>.Read(EDBReadBuffer buf, int len, bool async, FieldDescription? fieldDescription)
        {
            var range = await Read(buf, len, async, fieldDescription);
            return new(range.LowerBound, range.UpperBound - Period.FromDays(1));
        }

        public int ValidateAndGetLength(DateInterval value, ref EDBLengthCache? lengthCache, EDBParameter? parameter)
            => ValidateAndGetLengthRange(new EDBRange<LocalDate>(value.Start, value.End), ref lengthCache, parameter);

        public Task Write(
            DateInterval value, EDBWriteBuffer buf, EDBLengthCache? lengthCache, EDBParameter? parameter, bool async,
            CancellationToken cancellationToken = default)
            => WriteRange(new EDBRange<LocalDate>(value.Start, value.End), buf, lengthCache, parameter, async, cancellationToken);

#if NET6_0_OR_GREATER
        ValueTask<EDBRange<DateOnly>> IEDBTypeHandler<EDBRange<DateOnly>>.Read(
            EDBReadBuffer buf, int len, bool async, FieldDescription? fieldDescription)
            => ReadRange<DateOnly>(buf, len, async, fieldDescription);

        public int ValidateAndGetLength(EDBRange<DateOnly> value, ref EDBLengthCache? lengthCache, EDBParameter? parameter)
            => ValidateAndGetLengthRange(value, ref lengthCache, parameter);

        public Task Write(
            EDBRange<DateOnly> value,
            EDBWriteBuffer buf,
            EDBLengthCache? lengthCache,
            EDBParameter? parameter,
            bool async,
            CancellationToken cancellationToken = default)
            => WriteRange(value, buf, lengthCache, parameter, async, cancellationToken);
#endif
    }
}
