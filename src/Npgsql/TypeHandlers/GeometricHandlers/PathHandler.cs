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
    /// Type handler for the PostgreSQL geometric path segment type (open or closed).
    /// </summary>
    /// <remarks>
    /// http://www.postgresql.org/docs/current/static/datatype-geometric.html
    /// </remarks>
    [TypeMapping("path", EDBDbType.Path, typeof(EDBPath))]
    internal class PathHandler : TypeHandler<EDBPath>,
        IChunkingTypeReader<EDBPath>, IChunkingTypeWriter
    {
        #region State

        EDBPath _value;
        EDBBuffer _buf;
        int _index;

        #endregion

        #region Read

        public void PrepareRead(EDBBuffer buf, int len, FieldDescription fieldDescription)
        {
            _buf = buf;
            _index = -1;
        }

        public bool Read(out EDBPath result)
        {
            result = default(EDBPath);

            if (_index == -1)
            {
                if (_buf.ReadBytesLeft < 5) { return false; }

                bool open;
                var openByte = _buf.ReadByte();
                switch (openByte) {
                    case 1:
                        open = false;
                        break;
                    case 0:
                        open = true;
                        break;
                    default:
                        throw new Exception("Error decoding binary geometric path: bad open byte");
                }
                var numPoints = _buf.ReadInt32();
                _value = new EDBPath(numPoints, open);
                _index = 0;
            }

            for (; _index < _value.Capacity; _index++)
            {
                if (_buf.ReadBytesLeft < 16) { return false; }
                _value.Add(new EDBPoint(_buf.ReadDouble(), _buf.ReadDouble()));
            }
            result = _value;
            _value = default(EDBPath);
            _buf = null;
            return true;
        }

        #endregion

        #region Write

        public int ValidateAndGetLength(object value, ref LengthCache lengthCache, EDBParameter parameter=null)
        {
            if (!(value is EDBPath))
                    throw CreateConversionException(value.GetType());
            return 5 + ((EDBPath)value).Count * 16;
        }

        public void PrepareWrite(object value, EDBBuffer buf, LengthCache lengthCache, EDBParameter parameter=null)
        {
            _buf = buf;
            _value = (EDBPath)value;
            _index = -1;
        }

        public bool Write(ref DirectBuffer directBuf)
        {
            if (_index == -1)
            {
                if (_buf.WriteSpaceLeft < 5) { return false; }
                _buf.WriteByte((byte)(_value.Open ? 0 : 1));
                _buf.WriteInt32(_value.Count);
                _index = 0;
            }

            for (; _index < _value.Count; _index++)
            {
                if (_buf.WriteSpaceLeft < 16) { return false; }
                var p = _value[_index];
                _buf.WriteDouble(p.X);
                _buf.WriteDouble(p.Y);
            }
            _buf = null;
            _value = default(EDBPath);
            return true;
        }

        #endregion
    }
}
