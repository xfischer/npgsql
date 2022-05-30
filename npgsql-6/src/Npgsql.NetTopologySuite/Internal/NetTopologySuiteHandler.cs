using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.Internal;
using EnterpriseDB.EDBClient.Internal.TypeHandling;
using EnterpriseDB.EDBClient.PostgresTypes;

namespace EnterpriseDB.EDBClient.NetTopologySuite.Internal
{
    partial class NetTopologySuiteHandler : EDBTypeHandler<Geometry>,
        IEDBTypeHandler<Point>,
        IEDBTypeHandler<LineString>,
        IEDBTypeHandler<Polygon>,
        IEDBTypeHandler<MultiPoint>,
        IEDBTypeHandler<MultiLineString>,
        IEDBTypeHandler<MultiPolygon>,
        IEDBTypeHandler<GeometryCollection>
    {
        readonly PostGisReader _reader;
        readonly PostGisWriter _writer;
        readonly LengthStream _lengthStream = new();

        internal NetTopologySuiteHandler(PostgresType postgresType, PostGisReader reader, PostGisWriter writer)
            : base(postgresType)
        {
            _reader = reader;
            _writer = writer;
        }

        #region Read

        public override ValueTask<Geometry> Read(EDBReadBuffer buf, int len, bool async, FieldDescription? fieldDescription = null)
            => ReadCore<Geometry>(buf, len);

        ValueTask<Point> IEDBTypeHandler<Point>.Read(EDBReadBuffer buf, int len, bool async, FieldDescription? fieldDescription)
            => ReadCore<Point>(buf, len);

        ValueTask<LineString> IEDBTypeHandler<LineString>.Read(EDBReadBuffer buf, int len, bool async, FieldDescription? fieldDescription)
            => ReadCore<LineString>(buf, len);

        ValueTask<Polygon> IEDBTypeHandler<Polygon>.Read(EDBReadBuffer buf, int len, bool async, FieldDescription? fieldDescription)
            => ReadCore<Polygon>(buf, len);

        ValueTask<MultiPoint> IEDBTypeHandler<MultiPoint>.Read(EDBReadBuffer buf, int len, bool async, FieldDescription? fieldDescription)
            => ReadCore<MultiPoint>(buf, len);

        ValueTask<MultiLineString> IEDBTypeHandler<MultiLineString>.Read(EDBReadBuffer buf, int len, bool async, FieldDescription? fieldDescription)
            => ReadCore<MultiLineString>(buf, len);

        ValueTask<MultiPolygon> IEDBTypeHandler<MultiPolygon>.Read(EDBReadBuffer buf, int len, bool async, FieldDescription? fieldDescription)
            => ReadCore<MultiPolygon>(buf, len);

        ValueTask<GeometryCollection> IEDBTypeHandler<GeometryCollection>.Read(EDBReadBuffer buf, int len, bool async, FieldDescription? fieldDescription)
            => ReadCore<GeometryCollection>(buf, len);

        ValueTask<T> ReadCore<T>(EDBReadBuffer buf, int len)
            where T : Geometry
            => new((T)_reader.Read(buf.GetStream(len, false)));

        #endregion

        #region ValidateAndGetLength

        public override int ValidateAndGetLength(Geometry value, ref EDBLengthCache? lengthCache, EDBParameter? parameter)
            => ValidateAndGetLengthCore(value);

        int IEDBTypeHandler<Point>.ValidateAndGetLength(Point value, ref EDBLengthCache? lengthCache, EDBParameter? parameter)
            => ValidateAndGetLength(value, ref lengthCache, parameter);

        int IEDBTypeHandler<LineString>.ValidateAndGetLength(LineString value, ref EDBLengthCache? lengthCache, EDBParameter? parameter)
            => ValidateAndGetLength(value, ref lengthCache, parameter);

        int IEDBTypeHandler<Polygon>.ValidateAndGetLength(Polygon value, ref EDBLengthCache? lengthCache, EDBParameter? parameter)
            => ValidateAndGetLength(value, ref lengthCache, parameter);

        int IEDBTypeHandler<MultiPoint>.ValidateAndGetLength(MultiPoint value, ref EDBLengthCache? lengthCache, EDBParameter? parameter)
            => ValidateAndGetLength(value, ref lengthCache, parameter);

        int IEDBTypeHandler<MultiLineString>.ValidateAndGetLength(MultiLineString value, ref EDBLengthCache? lengthCache, EDBParameter? parameter)
            => ValidateAndGetLength(value, ref lengthCache, parameter);

        int IEDBTypeHandler<MultiPolygon>.ValidateAndGetLength(MultiPolygon value, ref EDBLengthCache? lengthCache, EDBParameter? parameter)
            => ValidateAndGetLength(value, ref lengthCache, parameter);

        int IEDBTypeHandler<GeometryCollection>.ValidateAndGetLength(GeometryCollection value, ref EDBLengthCache? lengthCache, EDBParameter? parameter)
            => ValidateAndGetLength(value, ref lengthCache, parameter);

        int ValidateAndGetLengthCore(Geometry value)
        {
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

        public override Task Write(Geometry value, EDBWriteBuffer buf, EDBLengthCache? lengthCache, EDBParameter? parameter, bool async, CancellationToken cancellationToken = default)
            => WriteCore(value, buf);

        Task IEDBTypeHandler<Point>.Write(Point value, EDBWriteBuffer buf, EDBLengthCache? lengthCache, EDBParameter? parameter, bool async, CancellationToken cancellationToken)
            => WriteCore(value, buf);

        Task IEDBTypeHandler<LineString>.Write(LineString value, EDBWriteBuffer buf, EDBLengthCache? lengthCache, EDBParameter? parameter, bool async, CancellationToken cancellationToken)
            => WriteCore(value, buf);

        Task IEDBTypeHandler<Polygon>.Write(Polygon value, EDBWriteBuffer buf, EDBLengthCache? lengthCache, EDBParameter? parameter, bool async, CancellationToken cancellationToken)
            => WriteCore(value, buf);

        Task IEDBTypeHandler<MultiPoint>.Write(MultiPoint value, EDBWriteBuffer buf, EDBLengthCache? lengthCache, EDBParameter? parameter, bool async, CancellationToken cancellationToke)
            => WriteCore(value, buf);

        Task IEDBTypeHandler<MultiLineString>.Write(MultiLineString value, EDBWriteBuffer buf, EDBLengthCache? lengthCache, EDBParameter? parameter, bool async, CancellationToken cancellationToken)
            => WriteCore(value, buf);

        Task IEDBTypeHandler<MultiPolygon>.Write(MultiPolygon value, EDBWriteBuffer buf, EDBLengthCache? lengthCache, EDBParameter? parameter, bool async, CancellationToken cancellationToken)
            => WriteCore(value, buf);

        Task IEDBTypeHandler<GeometryCollection>.Write(GeometryCollection value, EDBWriteBuffer buf, EDBLengthCache? lengthCache, EDBParameter? parameter, bool async, CancellationToken cancellationToken)
            => WriteCore(value, buf);

        Task WriteCore(Geometry value, EDBWriteBuffer buf)
        {
            _writer.Write(value, buf.GetStream());
            return Task.CompletedTask;
        }

        #endregion
    }
}
