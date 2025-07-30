using System.Threading.Tasks;
using EDBTypes;
using NUnit.Framework;

namespace EnterpriseDB.EDBClient.Tests.Types;

/// <summary>
/// Tests on PostgreSQL geometric types
/// </summary>
/// <remarks>
/// https://www.postgresql.org/docs/current/static/datatype-geometric.html
/// </remarks>
class GeometricTypeTests(MultiplexingMode multiplexingMode) : MultiplexingTestBase(multiplexingMode)
{
    [Test]
    public Task Point()
        => AssertType(new EDBPoint(1.2, 3.4), "(1.2,3.4)", "point", EDBDbType.Point);

    [Test]
    public Task Line()
        => AssertType(new EDBLine(1, 2, 3), "{1,2,3}", "line", EDBDbType.Line);

    [Test]
    public Task LineSegment()
        => AssertType(new EDBLSeg(1, 2, 3, 4), "[(1,2),(3,4)]", "lseg", EDBDbType.LSeg);

    [Test]
    public async Task Box()
    {
        await AssertType(
            new EDBBox(top: 3, right: 4, bottom: 1, left: 2),
            "(4,3),(2,1)",
            "box",
            EDBDbType.Box,
            skipArrayCheck: true); // Uses semicolon instead of comma as separator

        await AssertType(
            new EDBBox(top: -10, right: 0, bottom: -20, left: -10),
            "(0,-10),(-10,-20)",
            "box",
            EDBDbType.Box,
            skipArrayCheck: true); // Uses semicolon instead of comma as separator

        await AssertType(
            new EDBBox(top: 1, right: 2, bottom: 3, left: 4),
            "(4,3),(2,1)",
            "box",
            EDBDbType.Box,
            skipArrayCheck: true); // Uses semicolon instead of comma as separator

        var swapped = new EDBBox(top: -20, right: -10, bottom: -10, left: 0);

        await AssertType(
            swapped,
            "(0,-10),(-10,-20)",
            "box",
            EDBDbType.Box,
            skipArrayCheck: true); // Uses semicolon instead of comma as separator

        await AssertType(
            swapped with { UpperRight = new EDBPoint(-20,-10) },
            "(-10,-10),(-20,-20)",
            "box",
            EDBDbType.Box,
            skipArrayCheck: true); // Uses semicolon instead of comma as separator

        await AssertType(
            swapped with { LowerLeft = new EDBPoint(10, 10) },
            "(10,10),(0,-10)",
            "box",
            EDBDbType.Box,
            skipArrayCheck: true); // Uses semicolon instead of comma as separator
    }

    [Test]
    public async Task Box_array()
    {
        var data = new[]
        {
            new EDBBox(top: 3, right: 4, bottom: 1, left: 2),
            new EDBBox(top: 5, right: 6, bottom: 3, left: 4),
            new EDBBox(top: -10, right: 0, bottom: -20, left: -10)
        };

        await AssertType(
            data,
            "{(4,3),(2,1);(6,5),(4,3);(0,-10),(-10,-20)}",
            "box[]",
            EDBDbType.Box | EDBDbType.Array
            );

        var swappedData = new[]
        {
            new EDBBox(top: 1, right: 2, bottom: 3, left: 4),
            new EDBBox(top: 3, right: 4, bottom: 5, left: 6),
            new EDBBox(top: -20, right: -10, bottom: -10, left: 0)
        };

        await AssertType(
            swappedData,
            "{(4,3),(2,1);(6,5),(4,3);(0,-10),(-10,-20)}",
            "box[]",
            EDBDbType.Box | EDBDbType.Array
            );
    }

    [Test]
    public Task Path_closed()
        => AssertType(
            new EDBPath([new EDBPoint(1, 2), new EDBPoint(3, 4)], false),
            "((1,2),(3,4))",
            "path",
            EDBDbType.Path);

    [Test]
    public Task Path_open()
        => AssertType(
            new EDBPath([new EDBPoint(1, 2), new EDBPoint(3, 4)], true),
            "[(1,2),(3,4)]",
            "path",
            EDBDbType.Path);

    [Test]
    public Task Polygon()
        => AssertType(
            new EDBPolygon(new EDBPoint(1, 2), new EDBPoint(3, 4)),
            "((1,2),(3,4))",
            "polygon",
            EDBDbType.Polygon);

    [Test]
    public Task Circle()
        => AssertType(
            new EDBCircle(1, 2, 0.5),
            "<(1,2),0.5>",
            "circle",
            EDBDbType.Circle);
}
