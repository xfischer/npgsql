using System;
using System.Data;
using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.PostgresTypes;
using EnterpriseDB.EDBClient.TypeHandling;
using EnterpriseDB.EDBClient.TypeMapping;
using EDBTypes;

namespace EnterpriseDB.EDBClient.TypeHandlers.NumericHandlers
{
    /// <summary>
    /// A type handler for the PostgreSQL money data type.
    /// </summary>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/static/datatype-money.html.
    ///
    /// The type handler API allows customizing EnterpriseDB.EDBClient's behavior in powerful ways. However, although it is public, it
    /// should be considered somewhat unstable, and  may change in breaking ways, including in non-major releases.
    /// Use it at your own risk.
    /// </remarks>
    [TypeMapping("money", EDBDbType.Money, dbType: DbType.Currency)]
    public class MoneyHandler : EDBSimpleTypeHandler<decimal>
    {
        const int MoneyScale = 2;

        /// <inheritdoc />
        public MoneyHandler(PostgresType postgresType) : base(postgresType) {}

        /// <inheritdoc />
        public override decimal Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription = null)
            => new DecimalRaw(buf.ReadInt64()) { Scale = MoneyScale }.Value;

        /// <inheritdoc />
        public override int ValidateAndGetLength(decimal value, EDBParameter? parameter)
            => value < -92233720368547758.08M || value > 92233720368547758.07M
                ? throw new OverflowException($"The supplied value ({value}) is outside the range for a PostgreSQL money value.")
                : 8;

        /// <inheritdoc />
        public override void Write(decimal value, EDBWriteBuffer buf, EDBParameter? parameter)
        {
            var raw = new DecimalRaw(value);

            var scaleDifference = MoneyScale - raw.Scale;
            if (scaleDifference > 0)
                DecimalRaw.Multiply(ref raw, DecimalRaw.Powers10[scaleDifference]);
            else
            {
                value = Math.Round(value, MoneyScale, MidpointRounding.AwayFromZero);
                raw = new DecimalRaw(value);
            }

            var result = (long)raw.Mid << 32 | raw.Low;
            if (raw.Negative) result = -result;
            buf.WriteInt64(result);
        }
    }
}
