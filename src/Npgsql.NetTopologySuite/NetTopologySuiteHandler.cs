#region License
// The PostgreSQL License
//
// Copyright (C) 2018 The EnterpriseDB.EDBClient Development Team
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

using GeoAPI.Geometries;
using GeoAPI.IO;
using NetTopologySuite.Geometries;
using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.TypeHandling;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace EnterpriseDB.EDBClient.NetTopologySuite
{
    public class NetTopologySuiteHandlerFactory : EDBTypeHandlerFactory<IGeometry>
    {
        readonly IBinaryGeometryReader _reader;
        readonly IBinaryGeometryWriter _writer;

        internal NetTopologySuiteHandlerFactory(IBinaryGeometryReader reader, IBinaryGeometryWriter writer)
        {
            _reader = reader;
            _writer = writer;
        }

        protected override EDBTypeHandler<IGeometry> Create(EDBConnection conn)
            => new NetTopologySuiteHandler(_reader, _writer);
    }

    class NetTopologySuiteHandler : EDBTypeHandler<IGeometry>, IEDBTypeHandler<Geometry>,
        IEDBTypeHandler<IPoint>, IEDBTypeHandler<Point>,
        IEDBTypeHandler<ILineString>, IEDBTypeHandler<LineString>,
        IEDBTypeHandler<IPolygon>, IEDBTypeHandler<Polygon>,
        IEDBTypeHandler<IMultiPoint>, IEDBTypeHandler<MultiPoint>,
        IEDBTypeHandler<IMultiLineString>, IEDBTypeHandler<MultiLineString>,
        IEDBTypeHandler<IMultiPolygon>, IEDBTypeHandler<MultiPolygon>,
        IEDBTypeHandler<IGeometryCollection>, IEDBTypeHandler<GeometryCollection>
    {
        readonly IBinaryGeometryReader _reader;
        readonly IBinaryGeometryWriter _writer;
        LengthStream _lengthStream;

        internal NetTopologySuiteHandler(IBinaryGeometryReader reader, IBinaryGeometryWriter writer)
        {
            _reader = reader;
            _writer = writer;
        }

        #region Read

        public override ValueTask<IGeometry> Read(EDBReadBuffer buf, int len, bool async, FieldDescription fieldDescription = null)
            => ReadCore<IGeometry>(buf, len);

        ValueTask<Geometry> IEDBTypeHandler<Geometry>.Read(EDBReadBuffer buf, int len, bool async, FieldDescription fieldDescription)
            => ReadCore<Geometry>(buf, len);

        ValueTask<IPoint> IEDBTypeHandler<IPoint>.Read(EDBReadBuffer buf, int len, bool async, FieldDescription fieldDescription)
            => ReadCore<IPoint>(buf, len);

        ValueTask<Point> IEDBTypeHandler<Point>.Read(EDBReadBuffer buf, int len, bool async, FieldDescription fieldDescription)
            => ReadCore<Point>(buf, len);

        ValueTask<ILineString> IEDBTypeHandler<ILineString>.Read(EDBReadBuffer buf, int len, bool async, FieldDescription fieldDescription)
            => ReadCore<ILineString>(buf, len);

        ValueTask<LineString> IEDBTypeHandler<LineString>.Read(EDBReadBuffer buf, int len, bool async, FieldDescription fieldDescription)
            => ReadCore<LineString>(buf, len);

        ValueTask<IPolygon> IEDBTypeHandler<IPolygon>.Read(EDBReadBuffer buf, int len, bool async, FieldDescription fieldDescription)
            => ReadCore<IPolygon>(buf, len);

        ValueTask<Polygon> IEDBTypeHandler<Polygon>.Read(EDBReadBuffer buf, int len, bool async, FieldDescription fieldDescription)
            => ReadCore<Polygon>(buf, len);

        ValueTask<IMultiPoint> IEDBTypeHandler<IMultiPoint>.Read(EDBReadBuffer buf, int len, bool async, FieldDescription fieldDescription)
            => ReadCore<IMultiPoint>(buf, len);

        ValueTask<MultiPoint> IEDBTypeHandler<MultiPoint>.Read(EDBReadBuffer buf, int len, bool async, FieldDescription fieldDescription)
            => ReadCore<MultiPoint>(buf, len);

        ValueTask<IMultiLineString> IEDBTypeHandler<IMultiLineString>.Read(EDBReadBuffer buf, int len, bool async, FieldDescription fieldDescription)
            => ReadCore<IMultiLineString>(buf, len);

        ValueTask<MultiLineString> IEDBTypeHandler<MultiLineString>.Read(EDBReadBuffer buf, int len, bool async, FieldDescription fieldDescription)
            => ReadCore<MultiLineString>(buf, len);

        ValueTask<IMultiPolygon> IEDBTypeHandler<IMultiPolygon>.Read(EDBReadBuffer buf, int len, bool async, FieldDescription fieldDescription)
            => ReadCore<IMultiPolygon>(buf, len);

        ValueTask<MultiPolygon> IEDBTypeHandler<MultiPolygon>.Read(EDBReadBuffer buf, int len, bool async, FieldDescription fieldDescription)
            => ReadCore<MultiPolygon>(buf, len);

        ValueTask<IGeometryCollection> IEDBTypeHandler<IGeometryCollection>.Read(EDBReadBuffer buf, int len, bool async, FieldDescription fieldDescription)
            => ReadCore<IGeometryCollection>(buf, len);

        ValueTask<GeometryCollection> IEDBTypeHandler<GeometryCollection>.Read(EDBReadBuffer buf, int len, bool async, FieldDescription fieldDescription)
            => ReadCore<GeometryCollection>(buf, len);

        ValueTask<T> ReadCore<T>(EDBReadBuffer buf, int len)
            where T : IGeometry
            => new ValueTask<T>((T)_reader.Read(buf.GetStream(len, false)));

        #endregion

        #region ValidateAndGetLength

        public override int ValidateAndGetLength(IGeometry value, ref EDBLengthCache lengthCache, EDBParameter parameter)
            => ValidateAndGetLengthCore(value);

        int IEDBTypeHandler<Geometry>.ValidateAndGetLength(Geometry value, ref EDBLengthCache lengthCache, EDBParameter parameter)
            => ValidateAndGetLength(value, ref lengthCache, parameter);

        int IEDBTypeHandler<IPoint>.ValidateAndGetLength(IPoint value, ref EDBLengthCache lengthCache, EDBParameter parameter)
            => ValidateAndGetLength(value, ref lengthCache, parameter);

        int IEDBTypeHandler<Point>.ValidateAndGetLength(Point value, ref EDBLengthCache lengthCache, EDBParameter parameter)
            => ValidateAndGetLength(value, ref lengthCache, parameter);

        int IEDBTypeHandler<ILineString>.ValidateAndGetLength(ILineString value, ref EDBLengthCache lengthCache, EDBParameter parameter)
            => ValidateAndGetLength(value, ref lengthCache, parameter);

        int IEDBTypeHandler<LineString>.ValidateAndGetLength(LineString value, ref EDBLengthCache lengthCache, EDBParameter parameter)
            => ValidateAndGetLength(value, ref lengthCache, parameter);

        int IEDBTypeHandler<IPolygon>.ValidateAndGetLength(IPolygon value, ref EDBLengthCache lengthCache, EDBParameter parameter)
            => ValidateAndGetLength(value, ref lengthCache, parameter);

        int IEDBTypeHandler<Polygon>.ValidateAndGetLength(Polygon value, ref EDBLengthCache lengthCache, EDBParameter parameter)
            => ValidateAndGetLength(value, ref lengthCache, parameter);

        int IEDBTypeHandler<IMultiPoint>.ValidateAndGetLength(IMultiPoint value, ref EDBLengthCache lengthCache, EDBParameter parameter)
            => ValidateAndGetLength(value, ref lengthCache, parameter);

        int IEDBTypeHandler<MultiPoint>.ValidateAndGetLength(MultiPoint value, ref EDBLengthCache lengthCache, EDBParameter parameter)
            => ValidateAndGetLength(value, ref lengthCache, parameter);

        int IEDBTypeHandler<IMultiLineString>.ValidateAndGetLength(IMultiLineString value, ref EDBLengthCache lengthCache, EDBParameter parameter)
            => ValidateAndGetLength(value, ref lengthCache, parameter);

        int IEDBTypeHandler<MultiLineString>.ValidateAndGetLength(MultiLineString value, ref EDBLengthCache lengthCache, EDBParameter parameter)
            => ValidateAndGetLength(value, ref lengthCache, parameter);

        int IEDBTypeHandler<IMultiPolygon>.ValidateAndGetLength(IMultiPolygon value, ref EDBLengthCache lengthCache, EDBParameter parameter)
            => ValidateAndGetLength(value, ref lengthCache, parameter);

        int IEDBTypeHandler<MultiPolygon>.ValidateAndGetLength(MultiPolygon value, ref EDBLengthCache lengthCache, EDBParameter parameter)
            => ValidateAndGetLength(value, ref lengthCache, parameter);

        int IEDBTypeHandler<IGeometryCollection>.ValidateAndGetLength(IGeometryCollection value, ref EDBLengthCache lengthCache, EDBParameter parameter)
            => ValidateAndGetLength(value, ref lengthCache, parameter);

        int IEDBTypeHandler<GeometryCollection>.ValidateAndGetLength(GeometryCollection value, ref EDBLengthCache lengthCache, EDBParameter parameter)
            => ValidateAndGetLength(value, ref lengthCache, parameter);

        int ValidateAndGetLengthCore(IGeometry value)
        {
            if (_lengthStream == null)
                _lengthStream = new LengthStream();
            else
                _lengthStream.SetLength(0);

            _writer.Write(value, _lengthStream);
            return (int)_lengthStream.Length;
        }

        sealed class LengthStream : Stream
        {
            long _length;

            public override bool CanRead => false;

            public override bool CanSeek => false;

            public override bool CanWrite => true;

            public override long Length => _length;

            public override long Position
            {
                get => _length;
                set => throw new NotSupportedException();
            }

            public override void Flush()
            { }

            public override int Read(byte[] buffer, int offset, int count)
                => throw new NotSupportedException();

            public override long Seek(long offset, SeekOrigin origin)
                => throw new NotSupportedException();

            public override void SetLength(long value)
                => _length = value;

            public override void Write(byte[] buffer, int offset, int count)
                => _length += count;
        }

        #endregion

        #region Write

        public override Task Write(IGeometry value, EDBWriteBuffer buf, EDBLengthCache lengthCache, EDBParameter parameter, bool async)
            => WriteCore(value, buf, lengthCache, parameter, async);

        Task IEDBTypeHandler<Geometry>.Write(Geometry value, EDBWriteBuffer buf, EDBLengthCache lengthCache, EDBParameter parameter, bool async)
            => WriteCore(value, buf, lengthCache, parameter, async);

        Task IEDBTypeHandler<IPoint>.Write(IPoint value, EDBWriteBuffer buf, EDBLengthCache lengthCache, EDBParameter parameter, bool async)
            => WriteCore(value, buf, lengthCache, parameter, async);

        Task IEDBTypeHandler<Point>.Write(Point value, EDBWriteBuffer buf, EDBLengthCache lengthCache, EDBParameter parameter, bool async)
            => WriteCore(value, buf, lengthCache, parameter, async);

        Task IEDBTypeHandler<ILineString>.Write(ILineString value, EDBWriteBuffer buf, EDBLengthCache lengthCache, EDBParameter parameter, bool async)
            => WriteCore(value, buf, lengthCache, parameter, async);

        Task IEDBTypeHandler<LineString>.Write(LineString value, EDBWriteBuffer buf, EDBLengthCache lengthCache, EDBParameter parameter, bool async)
            => WriteCore(value, buf, lengthCache, parameter, async);

        Task IEDBTypeHandler<IPolygon>.Write(IPolygon value, EDBWriteBuffer buf, EDBLengthCache lengthCache, EDBParameter parameter, bool async)
            => WriteCore(value, buf, lengthCache, parameter, async);

        Task IEDBTypeHandler<Polygon>.Write(Polygon value, EDBWriteBuffer buf, EDBLengthCache lengthCache, EDBParameter parameter, bool async)
            => WriteCore(value, buf, lengthCache, parameter, async);

        Task IEDBTypeHandler<IMultiPoint>.Write(IMultiPoint value, EDBWriteBuffer buf, EDBLengthCache lengthCache, EDBParameter parameter, bool async)
            => WriteCore(value, buf, lengthCache, parameter, async);

        Task IEDBTypeHandler<MultiPoint>.Write(MultiPoint value, EDBWriteBuffer buf, EDBLengthCache lengthCache, EDBParameter parameter, bool async)
            => WriteCore(value, buf, lengthCache, parameter, async);

        Task IEDBTypeHandler<IMultiLineString>.Write(IMultiLineString value, EDBWriteBuffer buf, EDBLengthCache lengthCache, EDBParameter parameter, bool async)
            => WriteCore(value, buf, lengthCache, parameter, async);

        Task IEDBTypeHandler<MultiLineString>.Write(MultiLineString value, EDBWriteBuffer buf, EDBLengthCache lengthCache, EDBParameter parameter, bool async)
            => WriteCore(value, buf, lengthCache, parameter, async);

        Task IEDBTypeHandler<IMultiPolygon>.Write(IMultiPolygon value, EDBWriteBuffer buf, EDBLengthCache lengthCache, EDBParameter parameter, bool async)
            => WriteCore(value, buf, lengthCache, parameter, async);

        Task IEDBTypeHandler<MultiPolygon>.Write(MultiPolygon value, EDBWriteBuffer buf, EDBLengthCache lengthCache, EDBParameter parameter, bool async)
            => WriteCore(value, buf, lengthCache, parameter, async);

        Task IEDBTypeHandler<IGeometryCollection>.Write(IGeometryCollection value, EDBWriteBuffer buf, EDBLengthCache lengthCache, EDBParameter parameter, bool async)
            => WriteCore(value, buf, lengthCache, parameter, async);

        Task IEDBTypeHandler<GeometryCollection>.Write(GeometryCollection value, EDBWriteBuffer buf, EDBLengthCache lengthCache, EDBParameter parameter, bool async)
            => WriteCore(value, buf, lengthCache, parameter, async);

        Task WriteCore(IGeometry value, EDBWriteBuffer buf, EDBLengthCache lengthCache, EDBParameter parameter, bool async)
        {
            _writer.Write(value, buf.GetStream());
#if NET45
            return Task.Delay(0);
#else
            return Task.CompletedTask;
#endif
        }

        #endregion
    }
}
