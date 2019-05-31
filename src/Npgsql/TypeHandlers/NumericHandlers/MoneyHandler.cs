using System;
using System.Data;
using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.TypeHandling;
using EnterpriseDB.EDBClient.TypeMapping;
using EDBTypes;

namespace EnterpriseDB.EDBClient.TypeHandlers.NumericHandlers
{
    /// <remarks>
    /// http://www.postgresql.org/docs/current/static/datatype-money.html
    /// </remarks>
    [TypeMapping("money", EDBDbType.Money, dbType: DbType.Currency)]
    class MoneyHandler : EDBSimpleTypeHandler<decimal>
    {
        const int MoneyScale = 2;

        public override decimal Read(EDBReadBuffer buf, int len, FieldDescription fieldDescription = null)
        {
            return new DecimalRaw(buf.ReadInt64()) { Scale = MoneyScale }.Value;
        }

        public override int ValidateAndGetLength(decimal value, EDBParameter parameter)
            => value < -92233720368547758.08M || value > 92233720368547758.07M
                ? throw new OverflowException($"The supplied value ({value}) is outside the range for a PostgreSQL money value.")
                : 8;

        public override void Write(decimal value, EDBWriteBuffer buf, EDBParameter parameter)
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

            var result = (long)raw.Mid << 32 | (long)raw.Low;
            if (raw.Negative) result = -result;
            buf.WriteInt64(result);
        }
    }
}
