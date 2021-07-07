using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore.Storage;
using EDBTypes;
using EnterpriseDB.EDBClient;

namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    public class NpgsqlPointTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlPointTypeMapping() : base("point", typeof(EDBPoint), EDBDbType.Point) {}

        protected NpgsqlPointTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, EDBDbType.Point) {}

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NpgsqlPointTypeMapping(parameters);

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            var point = (EDBPoint)value;
            return FormattableString.Invariant($"POINT '({point.X:G17},{point.Y:G17})'");
        }

        public override Expression GenerateCodeLiteral(object value)
        {
            var point = (EDBPoint)value;
            return Expression.New(Constructor, Expression.Constant(point.X), Expression.Constant(point.Y));
        }

        static readonly ConstructorInfo Constructor =
            typeof(EDBPoint).GetConstructor(new[] { typeof(double), typeof(double) });
    }

    public class NpgsqlLineTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlLineTypeMapping() : base("line", typeof(EDBLine), EDBDbType.Line) {}

        protected NpgsqlLineTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, EDBDbType.Line) {}

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NpgsqlLineTypeMapping(parameters);

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            var line = (EDBLine)value;
            var a = line.A.ToString("G17", CultureInfo.InvariantCulture);
            var b = line.B.ToString("G17", CultureInfo.InvariantCulture);
            var c = line.C.ToString("G17", CultureInfo.InvariantCulture);
            return $"LINE '{{{a},{b},{c}}}'";
        }

        public override Expression GenerateCodeLiteral(object value)
        {
            var line = (EDBLine)value;
            return Expression.New(
                Constructor,
                Expression.Constant(line.A), Expression.Constant(line.B), Expression.Constant(line.C));
        }

        static readonly ConstructorInfo Constructor =
            typeof(EDBLine).GetConstructor(new[] { typeof(double), typeof(double), typeof(double) });
    }

    public class NpgsqlLineSegmentTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlLineSegmentTypeMapping() : base("lseg", typeof(EDBLSeg), EDBDbType.LSeg) {}

        protected NpgsqlLineSegmentTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, EDBDbType.LSeg) {}

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NpgsqlLineSegmentTypeMapping(parameters);

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            var lseg = (EDBLSeg)value;
            return FormattableString.Invariant($"LSEG '[({lseg.Start.X:G17},{lseg.Start.Y:G17}),({lseg.End.X:G17},{lseg.End.Y:G17})]'");
        }

        public override Expression GenerateCodeLiteral(object value)
        {
            var lseg = (EDBLSeg)value;
            return Expression.New(
                Constructor,
                Expression.Constant(lseg.Start.X), Expression.Constant(lseg.Start.Y),
                Expression.Constant(lseg.End.X), Expression.Constant(lseg.End.Y));
        }

        static readonly ConstructorInfo Constructor =
            typeof(EDBLSeg).GetConstructor(new[] { typeof(double), typeof(double), typeof(double), typeof(double) });
    }

    public class NpgsqlBoxTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlBoxTypeMapping() : base("box", typeof(EDBBox), EDBDbType.Box) {}

        protected NpgsqlBoxTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, EDBDbType.Box) {}

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NpgsqlBoxTypeMapping(parameters);

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            var box = (EDBBox)value;
            return FormattableString.Invariant($"BOX '(({box.Right:G17},{box.Top:G17}),({box.Left:G17},{box.Bottom:G17}))'");
        }

        public override Expression GenerateCodeLiteral(object value)
        {
            var box = (EDBBox)value;
            return Expression.New(
                Constructor,
                Expression.Constant(box.Top), Expression.Constant(box.Right),
                Expression.Constant(box.Bottom), Expression.Constant(box.Left));
        }

        static readonly ConstructorInfo Constructor =
            typeof(EDBBox).GetConstructor(new[] { typeof(double), typeof(double), typeof(double), typeof(double) });
    }

    public class NpgsqlPathTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlPathTypeMapping() : base("path", typeof(EDBPath), EDBDbType.Path) {}

        protected NpgsqlPathTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, EDBDbType.Path) {}

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NpgsqlPathTypeMapping(parameters);

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            var path = (EDBPath)value;
            var sb = new StringBuilder();
            sb.Append("PATH '");
            sb.Append(path.Open ? '[' : '(');
            for (var i = 0; i < path.Count; i++)
            {
                sb.Append('(');
                sb.Append(path[i].X.ToString("G17", CultureInfo.InvariantCulture));
                sb.Append(',');
                sb.Append(path[i].Y.ToString("G17", CultureInfo.InvariantCulture));
                sb.Append(')');
                if (i < path.Count - 1)
                    sb.Append(',');
            }
            sb.Append(path.Open ? ']' : ')');
            sb.Append('\'');
            return sb.ToString();
        }

        public override Expression GenerateCodeLiteral(object value)
        {
            var path = (EDBPath)value;
            return Expression.New(
                Constructor,
                Expression.NewArrayInit(typeof(EDBPoint),
                    path.Select(p => Expression.New(
                        PointConstructor,
                        Expression.Constant(p.X), Expression.Constant(p.Y)))),
                Expression.Constant(path.Open));
        }

        static readonly ConstructorInfo Constructor =
            typeof(EDBPath).GetConstructor(new[] { typeof(IEnumerable<EDBPoint>), typeof(bool) });

        static readonly ConstructorInfo PointConstructor =
            typeof(EDBPoint).GetConstructor(new[] { typeof(double), typeof(double) });
    }

    public class NpgsqlPolygonTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlPolygonTypeMapping() : base("polygon", typeof(EDBPolygon), EDBDbType.Polygon) {}

        protected NpgsqlPolygonTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, EDBDbType.Polygon) {}

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NpgsqlPolygonTypeMapping(parameters);

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            var polygon = (EDBPolygon)value;
            var sb = new StringBuilder();
            sb.Append("POLYGON '(");
            for (var i = 0; i < polygon.Count; i++)
            {
                sb.Append('(');
                sb.Append(polygon[i].X.ToString("G17", CultureInfo.InvariantCulture));
                sb.Append(',');
                sb.Append(polygon[i].Y.ToString("G17", CultureInfo.InvariantCulture));
                sb.Append(')');
                if (i < polygon.Count - 1)
                    sb.Append(',');
            }
            sb.Append(")'");
            return sb.ToString();
        }

        public override Expression GenerateCodeLiteral(object value)
        {
            var polygon = (EDBPolygon)value;
            return Expression.New(
                Constructor,
                Expression.NewArrayInit(typeof(EDBPoint),
                    polygon.Select(p => Expression.New(
                        PointConstructor,
                        Expression.Constant(p.X), Expression.Constant(p.Y)))));
        }

        static readonly ConstructorInfo Constructor =
            typeof(EDBPolygon).GetConstructor(new[] { typeof(EDBPoint[]) });

        static readonly ConstructorInfo PointConstructor =
            typeof(EDBPoint).GetConstructor(new[] { typeof(double), typeof(double) });
    }

    public class NpgsqlCircleTypeMapping : NpgsqlTypeMapping
    {
        public NpgsqlCircleTypeMapping() : base("circle", typeof(EDBCircle), EDBDbType.Circle) {}

        protected NpgsqlCircleTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, EDBDbType.Circle) {}

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NpgsqlCircleTypeMapping(parameters);

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            var circle = (EDBCircle)value;
            return FormattableString.Invariant($"CIRCLE '<({circle.X:G17},{circle.Y:G17}),{circle.Radius:G17}>'");
        }

        public override Expression GenerateCodeLiteral(object value)
        {
            var circle = (EDBCircle)value;
            return Expression.New(
                Constructor,
                Expression.Constant(circle.X), Expression.Constant(circle.Y), Expression.Constant(circle.Radius));
        }

        static readonly ConstructorInfo Constructor =
            typeof(EDBCircle).GetConstructor(new[] { typeof(double), typeof(double), typeof(double) });
    }
}
