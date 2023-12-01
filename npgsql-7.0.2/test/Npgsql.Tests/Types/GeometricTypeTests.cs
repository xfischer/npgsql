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
[NonParallelizable] // EnterpriseDB
class GeometricTypeTests : MultiplexingTestBase
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
    public Task Box()
        => AssertType(new EDBBox(3, 4, 1, 2), "(4,3),(2,1)", "box", EDBDbType.Box);

    [Test]
    public Task Path_closed()
        => AssertType(
            new EDBPath(new[] {new EDBPoint(1, 2), new EDBPoint(3, 4)}, false),
            "((1,2),(3,4))",
            "path",
            EDBDbType.Path);

    [Test]
    public Task Path_open()
        => AssertType(
            new EDBPath(new[] { new EDBPoint(1, 2), new EDBPoint(3, 4) }, true),
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

    public GeometricTypeTests(MultiplexingMode multiplexingMode) : base(multiplexingMode) {}
}
