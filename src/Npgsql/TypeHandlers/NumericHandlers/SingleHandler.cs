#region License
// The PostgreSQL License
//
// Copyright (C) 2018 The EDB Development Team
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
//
// IN NO EVENT SHALL THE EDB DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE EDB DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// THE EDB DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE EDB DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.
#endregion

using EnterpriseDB.EDBClient.BackendMessages;
using EDBTypes;
using System.Data;
using JetBrains.Annotations;
using EnterpriseDB.EDBClient.TypeHandling;
using EnterpriseDB.EDBClient.TypeMapping;

namespace EnterpriseDB.EDBClient.TypeHandlers.NumericHandlers
{
    /// <remarks>
    /// http://www.postgresql.org/docs/current/static/datatype-numeric.html
    /// </remarks>
    [TypeMapping("real", EDBDbType.Real, DbType.Single, typeof(float))]
    class SingleHandler : EDBSimpleTypeHandler<float>, IEDBSimpleTypeHandler<double>
    {
        #region Read

        public override float Read(EDBReadBuffer buf, int len, FieldDescription fieldDescription = null)
            => buf.ReadSingle();

        double IEDBSimpleTypeHandler<double>.Read(EDBReadBuffer buf, int len, [CanBeNull] FieldDescription fieldDescription)
            => Read(buf, len, fieldDescription);

        #endregion Read

        #region Write

        public int ValidateAndGetLength(double value, EDBParameter parameter)
            => 4;

        public override int ValidateAndGetLength(float value, EDBParameter parameter)
            => 4;

        public void Write(double value, EDBWriteBuffer buf, EDBParameter parameter)
            => buf.WriteSingle((float)value);

        public override void Write(float value, EDBWriteBuffer buf, EDBParameter parameter)
            => buf.WriteSingle(value);

        #endregion Write
    }
}
