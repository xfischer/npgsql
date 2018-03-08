#region License
// The PostgreSQL License
//
// Copyright (C) 2017 The EnterpriseDB.EDBClient Development Team
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
//
// IN NO EVENT SHALL THE EnterpriseDB.EDBClient DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE EnterpriseDB.EDBClient DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// THE EnterpriseDB.EDBClient DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE EnterpriseDB.EDBClient DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.
#endregion

using System;
using EnterpriseDB.EDBClient.BackendMessages;
using EDBTypes;
using System.Data;
using EnterpriseDB.EDBClient.PostgresTypes;

namespace EnterpriseDB.EDBClient.TypeHandlers.NumericHandlers
{
    /// <remarks>
    /// http://www.postgresql.org/docs/current/static/datatype-money.html
    /// </remarks>
    [TypeMapping("money", EDBDbType.Money, dbType: DbType.Currency)]
    class MoneyHandler : SimpleTypeHandler<decimal>
    {
        internal MoneyHandler(PostgresType postgresType) : base(postgresType) { }

        public override decimal Read(ReadBuffer buf, int len, FieldDescription fieldDescription = null)
            => buf.ReadInt64() / 100m;

        public override int ValidateAndGetLength(object value, EDBParameter parameter = null)
        {
            decimal decimalValue;
            if (!(value is decimal))
            {
                var converted = Convert.ToDecimal(value);
                if (parameter == null)
                    throw CreateConversionButNoParamException(value.GetType());
                decimalValue = converted;
                parameter.ConvertedValue = converted;
            }
            else
                decimalValue = (decimal)value;

            if (decimalValue < -92233720368547758.08M || decimalValue > 92233720368547758.07M)
                throw new OverflowException("The supplied value (" + decimalValue + ") is outside the range for a PostgreSQL money value.");

            return 8;
        }

        protected override void Write(object value, WriteBuffer buf, EDBParameter parameter = null)
        {
            var v = (decimal)(parameter?.ConvertedValue ?? value);
            buf.WriteInt64((long)(Math.Round(v, 2, MidpointRounding.AwayFromZero) * 100m));
        }
    }
}
