#region License
// The PostgreSQL License
//
// Copyright (C) 2015 The  EnterpriseDB.EDBClient Development Team
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
//
// IN NO EVENT SHALL THE  EnterpriseDB.EDBClient DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE  EnterpriseDB.EDBClient DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// THE  EnterpriseDB.EDBClient DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE  EnterpriseDB.EDBClient DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using  EnterpriseDB.EDBClient.BackendMessages;
using EDBTypes;

namespace  EnterpriseDB.EDBClient.TypeHandlers.GeometricHandlers
{
    /// <summary>
    /// Type handler for the PostgreSQL geometric line segment type.
    /// </summary>
    /// <remarks>
    /// http://www.postgresql.org/docs/current/static/datatype-geometric.html
    /// </remarks>
    [TypeMapping("lseg", EDBDbType.LSeg, typeof(EDBLSeg))]
    internal class LineSegmentHandler : TypeHandler<EDBLSeg>,
        ISimpleTypeReader<EDBLSeg>, ISimpleTypeWriter,
        ISimpleTypeReader<string>
    {
        public EDBLSeg Read(EDBBuffer buf, int len, FieldDescription fieldDescription)
        {
            return new EDBLSeg(buf.ReadDouble(), buf.ReadDouble(), buf.ReadDouble(), buf.ReadDouble());
        }

        string ISimpleTypeReader<string>.Read(EDBBuffer buf, int len, FieldDescription fieldDescription)
        {
            return Read(buf, len, fieldDescription).ToString();
        }

        public int ValidateAndGetLength(object value, EDBParameter parameter)
        {
            if (!(value is EDBLSeg))
                throw CreateConversionException(value.GetType());
            return 32;
        }

        public void Write(object value, EDBBuffer buf, EDBParameter parameter)
        {
            var v = (EDBLSeg)value;
            buf.WriteDouble(v.Start.X);
            buf.WriteDouble(v.Start.Y);
            buf.WriteDouble(v.End.X);
            buf.WriteDouble(v.End.Y);
        }
    }
}
