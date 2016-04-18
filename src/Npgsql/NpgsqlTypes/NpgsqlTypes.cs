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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using  EnterpriseDB.EDBClient;

#pragma warning disable 1591

namespace EDBTypes
{
    /// <summary>
    /// Represents a PostgreSQL point type.
    /// </summary>
    /// <remarks>
    /// See http://www.postgresql.org/docs/current/static/datatype-geometric.html
    /// </remarks>
    public struct EDBPoint : IEquatable<EDBPoint>
    {
        static readonly Regex Regex = new Regex(@"\((-?\d+.?\d*),(-?\d+.?\d*)\)");

        public double X { get; set; }
        public double Y { get; set; }

        public EDBPoint(double x, double y)
            : this()
        {
            X = x;
            Y = y;
        }

        public bool Equals(EDBPoint other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            return obj is EDBPoint && Equals((EDBPoint) obj);
        }

        public static bool operator ==(EDBPoint x, EDBPoint y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(EDBPoint x, EDBPoint y)
        {
            return !(x == y);
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ PGUtil.RotateShift(Y.GetHashCode(), sizeof (int)/2);
        }

        public static EDBPoint Parse(string s)
        {
            var m = Regex.Match(s);
            if (!m.Success) {
                throw new FormatException("Not a valid point: " + s);
            }
            return new EDBPoint(Double.Parse(m.Groups[1].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat),
                                   Double.Parse(m.Groups[2].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat));
        }

        public override string ToString()
        {
            return String.Format(CultureInfo.InvariantCulture, "({0},{1})", X, Y);
        }
    }

    /// <summary>
    /// Represents a PostgreSQL line type.
    /// </summary>
    /// <remarks>
    /// See http://www.postgresql.org/docs/current/static/datatype-geometric.html
    /// </remarks>
    public struct EDBLine : IEquatable<EDBLine>
    {
        static readonly Regex Regex = new Regex(@"\{(-?\d+.?\d*),(-?\d+.?\d*),(-?\d+.?\d*)\}");

        public double A { get; set; }
        public double B { get; set; }
        public double C { get; set; }

        public EDBLine(double a, double b, double c)
            : this()
        {
            A = a;
            B = b;
            C = c;
        }

        public static EDBLine Parse(string s)
        {
            var m = Regex.Match(s);
            if (!m.Success) {
                throw new FormatException("Not a valid line: " + s);
            }
            return new EDBLine(
                Double.Parse(m.Groups[1].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat),
                Double.Parse(m.Groups[2].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat),
                Double.Parse(m.Groups[3].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat)
            );
        }

        public override String ToString()
        {
            return String.Format(CultureInfo.InvariantCulture, "{{{0},{1},{2}}}", A, B, C);
        }

        public override int GetHashCode()
        {
            return A.GetHashCode() * B.GetHashCode() * C.GetHashCode();
        }

        public bool Equals(EDBLine other)
        {
            return A == other.A && B == other.B && C == other.C;
        }

        public override bool Equals(object obj)
        {
            return obj is EDBLine && Equals((EDBLine)obj);
        }

        public static bool operator ==(EDBLine x, EDBLine y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(EDBLine x, EDBLine y)
        {
            return !(x == y);
        }
    }

    /// <summary>
    /// Represents a PostgreSQL Line Segment type.
    /// </summary>
    public struct EDBLSeg : IEquatable<EDBLSeg>
    {
        static readonly Regex Regex = new Regex(@"\[\((-?\d+.?\d*),(-?\d+.?\d*)\),\((-?\d+.?\d*),(-?\d+.?\d*)\)\]");

        public EDBPoint Start { get; set; }
        public EDBPoint End { get; set; }

        public EDBLSeg(EDBPoint start, EDBPoint end)
            : this()
        {
            Start = start;
            End = end;
        }

        public EDBLSeg(double startx, double starty, double endx, double endy) : this()
        {
            Start = new EDBPoint(startx, starty);
            End   = new EDBPoint(endx,   endy);
        }

        public static EDBLSeg Parse(string s)
        {
            var m = Regex.Match(s);
            if (!m.Success) {
                throw new FormatException("Not a valid line: " + s);
            }
            return new EDBLSeg(
                Double.Parse(m.Groups[1].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat),
                Double.Parse(m.Groups[2].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat),
                Double.Parse(m.Groups[3].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat),
                Double.Parse(m.Groups[4].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat)
            );

        }

        public override String ToString()
        {
            return String.Format(CultureInfo.InvariantCulture, "[{0},{1}]", Start, End);
        }

        public override int GetHashCode()
        {
            return
                Start.X.GetHashCode() ^ PGUtil.RotateShift(Start.Y.GetHashCode(), sizeof(int) / 4) ^
                PGUtil.RotateShift(End.X.GetHashCode(), sizeof(int) / 2) ^ PGUtil.RotateShift(End.Y.GetHashCode(), sizeof(int) * 3 / 4);
        }

        public bool Equals(EDBLSeg other)
        {
            return Start == other.Start && End == other.End;
        }

        public override bool Equals(object obj)
        {
            return obj is EDBLSeg && Equals((EDBLSeg)obj);
        }

        public static bool operator ==(EDBLSeg x, EDBLSeg y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(EDBLSeg x, EDBLSeg y)
        {
            return !(x == y);
        }
    }

    /// <summary>
    /// Represents a PostgreSQL box type.
    /// </summary>
    /// <remarks>
    /// See http://www.postgresql.org/docs/current/static/datatype-geometric.html
    /// </remarks>
    public struct EDBBox : IEquatable<EDBBox>
    {
        static readonly Regex Regex = new Regex(@"\((-?\d+.?\d*),(-?\d+.?\d*)\),\((-?\d+.?\d*),(-?\d+.?\d*)\)");

        public EDBPoint UpperRight { get; set; }
        public EDBPoint LowerLeft { get; set; }

        public EDBBox(EDBPoint upperRight, EDBPoint lowerLeft) : this()
        {
            UpperRight = upperRight;
            LowerLeft = lowerLeft;
        }

        public EDBBox(double top, double right, double bottom, double left)
            : this(new EDBPoint(right, top), new EDBPoint(left, bottom)) { }

        public double Left   { get { return LowerLeft.X;  } }
        public double Right  { get { return UpperRight.X; } }
        public double Bottom { get { return LowerLeft.Y;  } }
        public double Top    { get { return UpperRight.Y; } }
        public double Width  { get { return Right - Left; } }
        public double Height { get { return Top - Bottom; } }

        public bool IsEmpty
        {
            get { return Width == 0 || Height == 0; }
        }

        public bool Equals(EDBBox other)
        {
            return UpperRight == other.UpperRight && LowerLeft == other.LowerLeft;
        }

        public override bool Equals(object obj)
        {
            return obj is EDBBox && Equals((EDBBox) obj);
        }

        public static bool operator ==(EDBBox x, EDBBox y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(EDBBox x, EDBBox y)
        {
            return !(x == y);
        }

        public override string ToString()
        {
            return String.Format(CultureInfo.InvariantCulture, "{0},{1}", UpperRight, LowerLeft);
        }

        public static EDBBox Parse(string s)
        {
            var m = Regex.Match(s);
            return new EDBBox(
                new EDBPoint(Double.Parse(m.Groups[1].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat),
                                Double.Parse(m.Groups[2].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat)),
                new EDBPoint(Double.Parse(m.Groups[3].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat),
                                Double.Parse(m.Groups[4].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat))
            );
        }

        public override int GetHashCode()
        {
            return
                Top.GetHashCode() ^ PGUtil.RotateShift(Right.GetHashCode(), sizeof (int)/4) ^
                PGUtil.RotateShift(Bottom.GetHashCode(), sizeof (int)/2) ^
                PGUtil.RotateShift(LowerLeft.GetHashCode(), sizeof (int)*3/4);
        }
    }

    /// <summary>
    /// Represents a PostgreSQL Path type.
    /// </summary>
    public struct EDBPath : IList<EDBPoint>, IEquatable<EDBPath>
    {
        readonly List<EDBPoint> _points;
        public bool Open { get; set; }

        public EDBPath(IEnumerable<EDBPoint> points, bool open) : this()
        {
            _points = new List<EDBPoint>(points);
            Open = open;
        }

        public EDBPath(IEnumerable<EDBPoint> points) : this(points, false) {}
        public EDBPath(params EDBPoint[] points) : this(points, false) {}

        public EDBPath(bool open) : this()
        {
            _points = new List<EDBPoint>();
            Open = open;
        }

        public EDBPath(int capacity, bool open) : this()
        {
            _points = new List<EDBPoint>(capacity);
            Open = open;
        }

        public EDBPath(int capacity) : this(capacity, false) {}

        public EDBPoint this[int index]
        {
            get { return _points[index]; }
            set { _points[index] = value; }
        }

        public int Capacity { get { return _points.Capacity; } }
        public int Count { get { return _points.Count; } }
        public bool IsReadOnly { get { return false; } }

        public int IndexOf(EDBPoint item)
        {
            return _points.IndexOf(item);
        }

        public void Insert(int index, EDBPoint item)
        {
            _points.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _points.RemoveAt(index);
        }

        public void Add(EDBPoint item)
        {
            _points.Add(item);
        }

        public void Clear()
        {
            _points.Clear();
        }

        public bool Contains(EDBPoint item)
        {
            return _points.Contains(item);
        }

        public void CopyTo(EDBPoint[] array, int arrayIndex)
        {
            _points.CopyTo(array, arrayIndex);
        }

        public bool Remove(EDBPoint item)
        {
            return _points.Remove(item);
        }

        public IEnumerator<EDBPoint> GetEnumerator()
        {
            return _points.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Equals(EDBPath other)
        {
            if (Open != other.Open || Count != other.Count)
                return false;
            else if(ReferenceEquals(_points, other._points))//Short cut for shallow copies.
                return true;
            for (int i = 0; i != Count; ++i)
            {
                if (this[i] != other[i])
                {
                    return false;
                }
            }
            return true;
        }

        public override bool Equals(object obj)
        {
            return obj != null && obj is EDBPath && Equals((EDBPath) obj);
        }

        public static bool operator ==(EDBPath x, EDBPath y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(EDBPath x, EDBPath y)
        {
            return !(x == y);
        }

        public override int GetHashCode()
        {
            int ret = 266370105;//seed with something other than zero to make paths of all zeros hash differently.
            foreach (EDBPoint point in this)
            {
                //The ideal amount to shift each value is one that would evenly spread it throughout
                //the resultant bytes. Using the current result % 32 is essentially using a random value
                //but one that will be the same on subsequent calls.
                ret ^= PGUtil.RotateShift(point.GetHashCode(), ret%sizeof (int));
            }
            return Open ? ret : -ret;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(Open ? '[' : '(');
            int i;
            for (i = 0; i < _points.Count; i++)
            {
                var p = _points[i];
                sb.AppendFormat(CultureInfo.InvariantCulture, "({0},{1})", p.X, p.Y);
                if (i < _points.Count - 1) {
                    sb.Append(",");
                }
            }
            sb.Append(Open ? ']' : ')');
            return sb.ToString();
        }

        public static EDBPath Parse(string s)
        {
            bool open;
            switch (s[0])
            {
                case '[':
                    open = true;
                    break;
                case '(':
                    open = false;
                    break;
                default:
                    throw new Exception("Invalid path string: " + s);
            }
            Contract.Assume(s[s.Length - 1] == (open ? ']' : ')'));
            var result = new EDBPath(open);
            var i = 1;
            while (true)
            {
                var i2 = s.IndexOf(')', i);
                result.Add(EDBPoint.Parse(s.Substring(i, i2 - i + 1)));
                if (s[i2 + 1] != ',')
                    break;
                i = i2 + 2;
            }
            return result;
        }
    }

    /// <summary>
    /// Represents a PostgreSQL Polygon type.
    /// </summary>
    public struct EDBPolygon : IList<EDBPoint>, IEquatable<EDBPolygon>
    {
        private readonly List<EDBPoint> _points;

        public EDBPolygon(IEnumerable<EDBPoint> points)
        {
            _points = new List<EDBPoint>(points);
        }

        public EDBPolygon(params EDBPoint[] points) : this ((IEnumerable<EDBPoint>) points) {}

        public EDBPolygon(int capacity)
        {
            _points = new List<EDBPoint>(capacity);
        }

        public EDBPoint this[int index]
        {
            get { return _points[index]; }
            set { _points[index] = value; }
        }

        public int Capacity { get { return _points.Capacity; } }
        public int Count { get { return _points.Count; } }
        public bool IsReadOnly { get { return false; } }
        public int IndexOf(EDBPoint item)
        {
            return _points.IndexOf(item);
        }

        public void Insert(int index, EDBPoint item)
        {
            _points.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _points.RemoveAt(index);
        }

        public void Add(EDBPoint item)
        {
            _points.Add(item);
        }

        public void Clear()
        {
            _points.Clear();
        }

        public bool Contains(EDBPoint item)
        {
            return _points.Contains(item);
        }

        public void CopyTo(EDBPoint[] array, int arrayIndex)
        {
            _points.CopyTo(array, arrayIndex);
        }

        public bool Remove(EDBPoint item)
        {
            return _points.Remove(item);
        }

        public IEnumerator<EDBPoint> GetEnumerator()
        {
            return _points.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Equals(EDBPolygon other)
        {
            if (Count != other.Count)
                return false;
            if (ReferenceEquals(_points, other._points))
                return true;
            for (int i = 0; i != Count; ++i)
            {
                if (this[i] != other[i])
                {
                    return false;
                }
            }
            return true;
        }

        public override bool Equals(object obj)
        {
            return obj is EDBPolygon && Equals((EDBPolygon) obj);
        }

        public static bool operator ==(EDBPolygon x, EDBPolygon y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(EDBPolygon x, EDBPolygon y)
        {
            return !(x == y);
        }

        public override int GetHashCode()
        {
            int ret = 266370105;//seed with something other than zero to make paths of all zeros hash differently.
            foreach (EDBPoint point in this)
            {
                //The ideal amount to shift each value is one that would evenly spread it throughout
                //the resultant bytes. Using the current result % 32 is essentially using a random value
                //but one that will be the same on subsequent calls.
                ret ^= PGUtil.RotateShift(point.GetHashCode(), ret%sizeof (int));
            }
            return ret;
        }

        public static EDBPolygon Parse(string s)
        {
            var points = new List<EDBPoint>();
            var i = 1;
            while (true)
            {
                var i2 = s.IndexOf(')', i);
                points.Add(EDBPoint.Parse(s.Substring(i, i2 - i + 1)));
                if (s[i2 + 1] != ',')
                    break;
                i = i2 + 2;
            }
            return new EDBPolygon(points);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append('(');
            int i;
            for (i = 0; i < _points.Count; i++)
            {
                var p = _points[i];
                sb.AppendFormat(CultureInfo.InvariantCulture, "({0},{1})", p.X, p.Y);
                if (i < _points.Count - 1) {
                    sb.Append(",");
                }
            }
            sb.Append(')');
            return sb.ToString();
        }
    }

    /// <summary>
    /// Represents a PostgreSQL Circle type.
    /// </summary>
    public struct EDBCircle : IEquatable<EDBCircle>
    {
        static readonly Regex Regex = new Regex(@"<\((-?\d+.?\d*),(-?\d+.?\d*)\),(\d+.?\d*)>");

        public double X { get; set; }
        public double Y { get; set; }
        public double Radius { get; set; }

        public EDBCircle(EDBPoint center, double radius)
            : this()
        {
            X = center.X;
            Y = center.Y;
            Radius = radius;
        }

        public EDBCircle(double x, double y, double radius) : this()
        {
            X = x;
            Y = y;
            Radius = radius;
        }

        public EDBPoint Center
        {
            get { return new EDBPoint(X, Y); }
            set
            {
                X = value.X;
                Y = value.Y;
            }
        }

        public bool Equals(EDBCircle other)
        {
            return X == other.X && Y == other.Y && Radius == other.Radius;
        }

        public override bool Equals(object obj)
        {
            return obj is EDBCircle && Equals((EDBCircle) obj);
        }

        public static EDBCircle Parse(string s)
        {
            var m = Regex.Match(s);
            if (!m.Success) {
                throw new FormatException("Not a valid circle: " + s);
            }

            return new EDBCircle(
                Double.Parse(m.Groups[1].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat),
                Double.Parse(m.Groups[2].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat),
                Double.Parse(m.Groups[3].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat)
            );
        }

        public override String ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "<({0},{1}),{2}>", X, Y, Radius);
        }

        public static bool operator ==(EDBCircle x, EDBCircle y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(EDBCircle x, EDBCircle y)
        {
            return !(x == y);
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() * Y.GetHashCode() * Radius.GetHashCode();
        }
    }

    /// <summary>
    /// Represents a PostgreSQL inet type, which is a combination of an IPAddress and a
    /// subnet mask.
    /// </summary>
    /// <remarks>
    /// http://www.postgresql.org/docs/current/static/datatype-net-types.html
    /// </remarks>
    public struct EDBInet : IEquatable<EDBInet>
    {
        public IPAddress Address;
        public int Netmask;

        public EDBInet(IPAddress address, int netmask)
        {
            if (address.AddressFamily != AddressFamily.InterNetwork && address.AddressFamily != AddressFamily.InterNetworkV6)
                throw new ArgumentException("Only IPAddress of InterNetwork or InterNetworkV6 address families are accepted", "address");
            Contract.EndContractBlock();

            Address = address;
            Netmask = netmask;
        }

        public EDBInet(IPAddress address)
        {
            if (address.AddressFamily != AddressFamily.InterNetwork && address.AddressFamily != AddressFamily.InterNetworkV6)
                throw new ArgumentException("Only IPAddress of InterNetwork or InterNetworkV6 address families are accepted", "address");
            Contract.EndContractBlock();

            Address = address;
            Netmask = address.AddressFamily == AddressFamily.InterNetwork ? 32 : 128;
        }

        public EDBInet(string addr)
        {
            if (addr.IndexOf('/') > 0)
            {
                var addrbits = addr.Split('/');
                if (addrbits.GetUpperBound(0) != 1) {
                    throw new FormatException("Invalid number of parts in CIDR specification");
                }
                Address = IPAddress.Parse(addrbits[0]);
                Netmask = int.Parse(addrbits[1]);
            }
            else
            {
                Address = IPAddress.Parse(addr);
                Netmask = 32;
            }
        }

        public override String ToString()
        {
            if (Netmask != 32) {
                return string.Format("{0}/{1}", Address, Netmask);
            }
            return Address.ToString();
        }

        public static explicit operator IPAddress(EDBInet x)
        {
            if (x.Netmask != 32) {
                throw new InvalidCastException("Cannot cast CIDR network to address");
            }
            return x.Address;
        }

        public static implicit operator EDBInet(IPAddress ipaddress)
        {
            return new EDBInet(ipaddress);
        }

        public bool Equals(EDBInet other)
        {
            return Address.Equals(other.Address) && Netmask == other.Netmask;
        }

        public override bool Equals(object obj)
        {
            return obj != null && obj is EDBInet && Equals((EDBInet) obj);
        }

        public override int GetHashCode()
        {
            return PGUtil.RotateShift(Address.GetHashCode(), Netmask%32);
        }

        public static bool operator ==(EDBInet x, EDBInet y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(EDBInet x, EDBInet y)
        {
            return !(x == y);
        }
    }
}

#pragma warning restore 1591
