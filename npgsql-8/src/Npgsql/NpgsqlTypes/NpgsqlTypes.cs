using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions; // EDBMERGE: Not native AOT compliant, TODO remove and implement proper parsing

#pragma warning disable 1591

// ReSharper disable once CheckNamespace
namespace EDBTypes;

/// <summary>
/// Represents a PostgreSQL point type.
/// </summary>
/// <remarks>
/// See https://www.postgresql.org/docs/current/static/datatype-geometric.html
/// </remarks>
public struct EDBPoint : IEquatable<EDBPoint>
{
    public double X { get; set; }
    public double Y { get; set; }

    public EDBPoint(double x, double y)
        : this()
    {
        X = x;
        Y = y;
    }

    // ReSharper disable CompareOfFloatsByEqualityOperator
    public bool Equals(EDBPoint other) => X == other.X && Y == other.Y;
    // ReSharper restore CompareOfFloatsByEqualityOperator

    public override bool Equals(object? obj)
        => obj is EDBPoint point && Equals(point);

    public static bool operator ==(EDBPoint x, EDBPoint y) => x.Equals(y);

    public static bool operator !=(EDBPoint x, EDBPoint y) => !(x == y);

    public override int GetHashCode()
        => HashCode.Combine(X, Y);

    // EDBMERGE: Not native AOT compliant, TODO remove and implement proper parsing
    static readonly Regex Regex = new(@"\((-?\d+.?\d*),(-?\d+.?\d*)\)");
    public static EDBPoint Parse(string s)
    {
        var m = Regex.Match(s);
        if (!m.Success)
        {
            throw new FormatException("Not a valid point: " + s);
        }
        return new EDBPoint(double.Parse(m.Groups[1].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat),
            double.Parse(m.Groups[2].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat));
    }

    public override string ToString()
        => string.Format(CultureInfo.InvariantCulture, "({0},{1})", X, Y);
}

/// <summary>
/// Represents a PostgreSQL line type.
/// </summary>
/// <remarks>
/// See https://www.postgresql.org/docs/current/static/datatype-geometric.html
/// </remarks>
public struct EDBLine : IEquatable<EDBLine>
{
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

    // EDBMERGE: Not native AOT compliant, TODO remove and implement proper parsing
    static readonly Regex Regex = new(@"\{(-?\d+.?\d*),(-?\d+.?\d*),(-?\d+.?\d*)\}");
    public static EDBLine Parse(string s)
    {
        var m = Regex.Match(s);
        if (!m.Success)
            throw new FormatException("Not a valid line: " + s);
        return new EDBLine(
            double.Parse(m.Groups[1].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat),
            double.Parse(m.Groups[2].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat),
            double.Parse(m.Groups[3].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat)
        );
    }

    public override string ToString()
        => string.Format(CultureInfo.InvariantCulture, "{{{0},{1},{2}}}", A, B, C);

    public override int GetHashCode()
        => HashCode.Combine(A, B, C);

    public bool Equals(EDBLine other)
        => A == other.A && B == other.B && C == other.C;

    public override bool Equals(object? obj)
        => obj is EDBLine line && Equals(line);

    public static bool operator ==(EDBLine x, EDBLine y) => x.Equals(y);
    public static bool operator !=(EDBLine x, EDBLine y) => !(x == y);
}

/// <summary>
/// Represents a PostgreSQL Line Segment type.
/// </summary>
public struct EDBLSeg : IEquatable<EDBLSeg>
{
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

    // EDBMERGE: Not native AOT compliant, TODO remove and implement proper parsing
    static readonly Regex Regex = new(@"\[\((-?\d+.?\d*),(-?\d+.?\d*)\),\((-?\d+.?\d*),(-?\d+.?\d*)\)\]");
    public static EDBLSeg Parse(string s)
    {
        var m = Regex.Match(s);
        if (!m.Success)
        {
            throw new FormatException("Not a valid line: " + s);
        }
        return new EDBLSeg(
            double.Parse(m.Groups[1].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat),
            double.Parse(m.Groups[2].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat),
            double.Parse(m.Groups[3].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat),
            double.Parse(m.Groups[4].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat)
        );

    }

    public override string ToString()
        => string.Format(CultureInfo.InvariantCulture, "[{0},{1}]", Start, End);

    public override int GetHashCode()
        => HashCode.Combine(Start.X, Start.Y, End.X, End.Y);

    public bool Equals(EDBLSeg other)
        => Start == other.Start && End == other.End;

    public override bool Equals(object? obj)
        => obj is EDBLSeg seg && Equals(seg);

    public static bool operator ==(EDBLSeg x, EDBLSeg y) => x.Equals(y);
    public static bool operator !=(EDBLSeg x, EDBLSeg y) => !(x == y);
}

/// <summary>
/// Represents a PostgreSQL box type.
/// </summary>
/// <remarks>
/// See https://www.postgresql.org/docs/current/static/datatype-geometric.html
/// </remarks>
public struct EDBBox : IEquatable<EDBBox>
{
    EDBPoint _upperRight;
    public EDBPoint UpperRight
    {
        get => _upperRight;
        set
        {
            _upperRight = value;
            NormalizeBox();
        }
    }

    EDBPoint _lowerLeft;
    public EDBPoint LowerLeft
    {
        get => _lowerLeft;
        set
        {
            _lowerLeft = value;
            NormalizeBox();
        }
    }

    public EDBBox(EDBPoint upperRight, EDBPoint lowerLeft) : this()
    {
        _upperRight = upperRight;
        _lowerLeft = lowerLeft;
        NormalizeBox();
    }

    public EDBBox(double top, double right, double bottom, double left)
        : this(new EDBPoint(right, top), new EDBPoint(left, bottom)) { }

    public double Left => LowerLeft.X;
    public double Right => UpperRight.X;
    public double Bottom => LowerLeft.Y;
    public double Top => UpperRight.Y;
    public double Width => Right - Left;
    public double Height => Top - Bottom;

    public bool IsEmpty => Width == 0 || Height == 0;

    public bool Equals(EDBBox other)
        => UpperRight == other.UpperRight && LowerLeft == other.LowerLeft;

    public override bool Equals(object? obj)
        => obj is EDBBox box && Equals(box);

    public static bool operator ==(EDBBox x, EDBBox y) => x.Equals(y);
    public static bool operator !=(EDBBox x, EDBBox y) => !(x == y);

    // EDBMERGE: Not native AOT compliant, TODO remove and implement proper parsing
    static readonly Regex Regex = new(@"\((-?\d+.?\d*),(-?\d+.?\d*)\),\((-?\d+.?\d*),(-?\d+.?\d*)\)");
    public static EDBBox Parse(string s)
    {
        var m = Regex.Match(s);
        return new EDBBox(
            new EDBPoint(double.Parse(m.Groups[1].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat),
                double.Parse(m.Groups[2].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat)),
            new EDBPoint(double.Parse(m.Groups[3].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat),
                double.Parse(m.Groups[4].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat))
        );
    }

    // Swaps corners for isomorphic boxes, to mirror postgres behavior.
    // See: https://github.com/postgres/postgres/blob/af2324fabf0020e464b0268be9ef03e8f46ed84b/src/backend/utils/adt/geo_ops.c#L435-L447
    void NormalizeBox()
    {
        if (_upperRight.X < _lowerLeft.X)
            (_upperRight.X, _lowerLeft.X) = (_lowerLeft.X, _upperRight.X);

        if (_upperRight.Y < _lowerLeft.Y)
            (_upperRight.Y, _lowerLeft.Y) = (_lowerLeft.Y, _upperRight.Y);
    }

    public override string ToString()
        => string.Format(CultureInfo.InvariantCulture, "{0},{1}", UpperRight, LowerLeft);

    public override int GetHashCode()
        => HashCode.Combine(Top, Right, Bottom, LowerLeft);
}

/// <summary>
/// Represents a PostgreSQL Path type.
/// </summary>
public struct EDBPath : IList<EDBPoint>, IEquatable<EDBPath>
{
    readonly List<EDBPoint> _points;
    public bool Open { get; set; }

    public EDBPath()
        => _points = new();

    public EDBPath(IEnumerable<EDBPoint> points, bool open)
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
        get => _points[index];
        set => _points[index] = value;
    }

    public int Capacity => _points.Capacity;
    public int Count => _points.Count;
    public bool IsReadOnly => false;

    public int IndexOf(EDBPoint item) => _points.IndexOf(item);
    public void Insert(int index, EDBPoint item) => _points.Insert(index, item);
    public void RemoveAt(int index) => _points.RemoveAt(index);
    public void Add(EDBPoint item) => _points.Add(item);
    public void Clear() =>  _points.Clear();
    public bool Contains(EDBPoint item) => _points.Contains(item);
    public void CopyTo(EDBPoint[] array, int arrayIndex) =>  _points.CopyTo(array, arrayIndex);
    public bool Remove(EDBPoint item) =>  _points.Remove(item);
    public IEnumerator<EDBPoint> GetEnumerator() =>  _points.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public bool Equals(EDBPath other)
    {
        if (Open != other.Open || Count != other.Count)
            return false;
        if (ReferenceEquals(_points, other._points))//Short cut for shallow copies.
            return true;
        for (var i = 0; i != Count; ++i)
            if (this[i] != other[i])
                return false;
        return true;
    }

    public override bool Equals(object? obj)
        => obj is EDBPath path && Equals(path);

    public static bool operator ==(EDBPath x, EDBPath y) => x.Equals(y);
    public static bool operator !=(EDBPath x, EDBPath y) => !(x == y);

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(Open);

        foreach (var point in this)
        {
            hashCode.Add(point.X);
            hashCode.Add(point.Y);
        }

        return hashCode.ToHashCode();
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
            if (i < _points.Count - 1)
                sb.Append(",");
        }
        sb.Append(Open ? ']' : ')');
        return sb.ToString();
    }

    // EDBMERGE: Not native AOT compliant, TODO remove and implement proper parsing
    public static EDBPath Parse(string s)
    {
        var open = s[0] switch
        {
            '[' => true,
            '(' => false,
            _ => throw new Exception("Invalid path string: " + s)
        };
        Debug.Assert(s[s.Length - 1] == (open ? ']' : ')'));
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
public readonly struct EDBPolygon : IList<EDBPoint>, IEquatable<EDBPolygon>
{
    readonly List<EDBPoint> _points;
    public EDBPolygon()
        => _points = new();

    public EDBPolygon(IEnumerable<EDBPoint> points)
        => _points = new List<EDBPoint>(points);

    public EDBPolygon(params EDBPoint[] points) : this((IEnumerable<EDBPoint>) points) {}

    public EDBPolygon(int capacity)
        => _points = new List<EDBPoint>(capacity);

    public EDBPoint this[int index]
    {
        get => _points[index];
        set => _points[index] = value;
    }

    public int Capacity => _points.Capacity;
    public int Count => _points.Count;
    public bool IsReadOnly => false;

    public int IndexOf(EDBPoint item) => _points.IndexOf(item);
    public void Insert(int index, EDBPoint item) => _points.Insert(index, item);
    public void RemoveAt(int index) =>  _points.RemoveAt(index);
    public void Add(EDBPoint item) =>  _points.Add(item);
    public void Clear() =>  _points.Clear();
    public bool Contains(EDBPoint item) => _points.Contains(item);
    public void CopyTo(EDBPoint[] array, int arrayIndex) => _points.CopyTo(array, arrayIndex);
    public bool Remove(EDBPoint item) => _points.Remove(item);
    public IEnumerator<EDBPoint> GetEnumerator() => _points.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public bool Equals(EDBPolygon other)
    {
        if (Count != other.Count)
            return false;
        if (ReferenceEquals(_points, other._points))
            return true;
        for (var i = 0; i != Count; ++i)
            if (this[i] != other[i])
                return false;
        return true;
    }

    public override bool Equals(object? obj)
        => obj is EDBPolygon polygon && Equals(polygon);

    public static bool operator ==(EDBPolygon x, EDBPolygon y) => x.Equals(y);
    public static bool operator !=(EDBPolygon x, EDBPolygon y) => !(x == y);

    public override int GetHashCode()
    {
        var hashCode = new HashCode();

        foreach (var point in this)
        {
            hashCode.Add(point.X);
            hashCode.Add(point.Y);
        }

        return hashCode.ToHashCode();
    }

    // EDBMERGE: Not native AOT compliant, TODO remove and implement proper parsing
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
        get => new(X, Y);
        set => (X, Y) = (value.X, value.Y);
    }

    // ReSharper disable CompareOfFloatsByEqualityOperator
    public bool Equals(EDBCircle other)
        => X == other.X && Y == other.Y && Radius == other.Radius;
    // ReSharper restore CompareOfFloatsByEqualityOperator

    public override bool Equals(object? obj)
        => obj is EDBCircle circle && Equals(circle);

    // EDBMERGE: Not native AOT compliant, TODO remove and implement proper parsing
    static readonly Regex Regex = new(@"<\((-?\d+.?\d*),(-?\d+.?\d*)\),(\d+.?\d*)>");
    public static EDBCircle Parse(string s)
    {
        var m = Regex.Match(s);
        if (!m.Success)
            throw new FormatException("Not a valid circle: " + s);

        return new EDBCircle(
            double.Parse(m.Groups[1].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat),
            double.Parse(m.Groups[2].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat),
            double.Parse(m.Groups[3].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat)
        );
    }

    public override string ToString()
        => string.Format(CultureInfo.InvariantCulture, "<({0},{1}),{2}>", X, Y, Radius);

    public static bool operator ==(EDBCircle x, EDBCircle y) => x.Equals(y);
    public static bool operator !=(EDBCircle x, EDBCircle y) => !(x == y);

    public override int GetHashCode()
        => HashCode.Combine(X, Y, Radius);
}

/// <summary>
/// Represents a PostgreSQL inet type, which is a combination of an IPAddress and a
/// subnet mask.
/// </summary>
/// <remarks>
/// https://www.postgresql.org/docs/current/static/datatype-net-types.html
/// </remarks>
public readonly record struct EDBInet
{
    public IPAddress Address { get; }
    public byte Netmask { get; }

    public EDBInet(IPAddress address, byte netmask)
    {
        if (address.AddressFamily != AddressFamily.InterNetwork && address.AddressFamily != AddressFamily.InterNetworkV6)
            throw new ArgumentException("Only IPAddress of InterNetwork or InterNetworkV6 address families are accepted", nameof(address));

        Address = address;
        Netmask = netmask;
    }

    public EDBInet(IPAddress address)
        : this(address, (byte)(address.AddressFamily == AddressFamily.InterNetwork ? 32 : 128))
    {
    }

    public EDBInet(string addr)
        => (Address, Netmask) = addr.Split('/') switch
        {
            { Length: 2 } segments => (IPAddress.Parse(segments[0]), byte.Parse(segments[1])),
            { Length: 1 } segments => (IPAddress.Parse(segments[0]), (byte)32),
            _ => throw new FormatException("Invalid number of parts in CIDR specification")
        };

    public override string ToString()
        => (Address.AddressFamily == AddressFamily.InterNetwork && Netmask == 32) ||
           (Address.AddressFamily == AddressFamily.InterNetworkV6 && Netmask == 128)
            ? Address.ToString()
            : $"{Address}/{Netmask}";

    public static explicit operator IPAddress(EDBInet inet)
        => inet.Address;

    public static implicit operator EDBInet(IPAddress ip)
        => new(ip);

    public void Deconstruct(out IPAddress address, out byte netmask)
    {
        address = Address;
        netmask = Netmask;
    }
}

/// <summary>
/// Represents a PostgreSQL cidr type.
/// </summary>
/// <remarks>
/// https://www.postgresql.org/docs/current/static/datatype-net-types.html
/// </remarks>
public readonly record struct EDBCidr
{
    public IPAddress Address { get; }
    public byte Netmask { get; }

    public EDBCidr(IPAddress address, byte netmask)
    {
        if (address.AddressFamily != AddressFamily.InterNetwork && address.AddressFamily != AddressFamily.InterNetworkV6)
            throw new ArgumentException("Only IPAddress of InterNetwork or InterNetworkV6 address families are accepted", nameof(address));

        Address = address;
        Netmask = netmask;
    }

    public EDBCidr(string addr)
        => (Address, Netmask) = addr.Split('/') switch
        {
            { Length: 2 } segments => (IPAddress.Parse(segments[0]), byte.Parse(segments[1])),
            { Length: 1 } => throw new FormatException("Missing netmask"),
            _ => throw new FormatException("Invalid number of parts in CIDR specification")
        };

    public static implicit operator EDBInet(EDBCidr cidr)
        => new(cidr.Address, cidr.Netmask);

    public static explicit operator IPAddress(EDBCidr cidr)
        => cidr.Address;

    public override string ToString()
        => $"{Address}/{Netmask}";

    public void Deconstruct(out IPAddress address, out byte netmask)
    {
        address = Address;
        netmask = Netmask;
    }
}

/// <summary>
/// Represents a PostgreSQL tid value
/// </summary>
/// <remarks>
/// https://www.postgresql.org/docs/current/static/datatype-oid.html
/// </remarks>
public readonly struct EDBTid : IEquatable<EDBTid>
{
    /// <summary>
    /// Block number
    /// </summary>
    public uint BlockNumber { get; }

    /// <summary>
    /// Tuple index within block
    /// </summary>
    public ushort OffsetNumber { get; }

    public EDBTid(uint blockNumber, ushort offsetNumber)
    {
        BlockNumber = blockNumber;
        OffsetNumber = offsetNumber;
    }

    public bool Equals(EDBTid other)
        => BlockNumber == other.BlockNumber && OffsetNumber == other.OffsetNumber;

    public override bool Equals(object? o)
        => o is EDBTid tid && Equals(tid);

    public override int GetHashCode() => (int)BlockNumber ^ OffsetNumber;
    public static bool operator ==(EDBTid left, EDBTid right) => left.Equals(right);
    public static bool operator !=(EDBTid left, EDBTid right) => !(left == right);
    public override string ToString() => $"({BlockNumber},{OffsetNumber})";
}

#pragma warning restore 1591
