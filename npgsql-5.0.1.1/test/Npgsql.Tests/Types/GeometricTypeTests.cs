using System.Threading.Tasks;
using EDBTypes;
using NUnit.Framework;

namespace EnterpriseDB.EDBClient.Tests.Types
{
    /// <summary>
    /// Tests on PostgreSQL geometric types
    /// </summary>
    /// <remarks>
    /// https://www.postgresql.org/docs/current/static/datatype-geometric.html
    /// </remarks>
    class GeometricTypeTests : MultiplexingTestBase
    {
        [Test]
        public async Task Point()
        {
            using (var conn = await OpenConnectionAsync())
            {
                var expected = new EDBPoint(1.2, 3.4);
                var cmd = new EDBCommand("SELECT @p1, @p2", conn);
                var p1 = new EDBParameter("p1", EDBDbType.Point) {Value = expected};
                var p2 = new EDBParameter {ParameterName = "p2", Value = expected};
                Assert.That(p2.EDBDbType, Is.EqualTo(EDBDbType.Point));
                cmd.Parameters.Add(p1);
                cmd.Parameters.Add(p2);
                using (var reader = await cmd.ExecuteReaderAsync()) {
                    reader.Read();

                    for (var i = 0; i < cmd.Parameters.Count; i++)
                    {
                        Assert.That(reader.GetFieldType(i), Is.EqualTo(typeof(EDBPoint)));
                        var actual = reader.GetFieldValue<EDBPoint>(i);
                        AssertPointsEqual(actual, expected);
                    }
                }
            }
        }

        [Test]
        public async Task LineSegment()
        {
            using (var conn = await OpenConnectionAsync())
            {
                var expected = new EDBLSeg(1, 2, 3, 4);
                var cmd = new EDBCommand("SELECT @p1, @p2", conn);
                var p1 = new EDBParameter("p1", EDBDbType.LSeg) {Value = expected};
                var p2 = new EDBParameter {ParameterName = "p2", Value = expected};
                Assert.That(p2.EDBDbType, Is.EqualTo(EDBDbType.LSeg));
                cmd.Parameters.Add(p1);
                cmd.Parameters.Add(p2);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    reader.Read();

                    for (var i = 0; i < cmd.Parameters.Count; i++)
                    {
                        Assert.That(reader.GetFieldType(i), Is.EqualTo(typeof(EDBLSeg)));
                        var actual = reader.GetFieldValue<EDBLSeg>(i);
                        AssertPointsEqual(actual.Start, expected.Start);
                        AssertPointsEqual(actual.End, expected.End);
                    }
                }
            }
        }

        [Test]
        public async Task Box()
        {
            using (var conn = await OpenConnectionAsync())
            {
                var expected = new EDBBox(2, 4, 1, 3);
                var cmd = new EDBCommand("SELECT @p1, @p2", conn);
                var p1 = new EDBParameter("p1", EDBDbType.Box) {Value = expected};
                var p2 = new EDBParameter {ParameterName = "p2", Value = expected};
                Assert.That(p2.EDBDbType, Is.EqualTo(EDBDbType.Box));
                cmd.Parameters.Add(p1);
                cmd.Parameters.Add(p2);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    reader.Read();

                    for (var i = 0; i < cmd.Parameters.Count; i++)
                    {
                        Assert.That(reader.GetFieldType(i), Is.EqualTo(typeof(EDBBox)));
                        var actual = reader.GetFieldValue<EDBBox>(i);
                        AssertPointsEqual(actual.UpperRight, expected.UpperRight);
                    }
                }
            }
        }

        [Test]
        public async Task Path()
        {
            using (var conn = await OpenConnectionAsync())
            {
                var expectedOpen = new EDBPath(new[] {new EDBPoint(1, 2), new EDBPoint(3, 4)}, true);
                var expectedClosed = new EDBPath(new[] {new EDBPoint(1, 2), new EDBPoint(3, 4)}, false);
                var cmd = new EDBCommand("SELECT @p1, @p2, @p3", conn);
                var p1 = new EDBParameter("p1", EDBDbType.Path) {Value = expectedOpen};
                var p2 = new EDBParameter("p2", EDBDbType.Path) {Value = expectedClosed};
                var p3 = new EDBParameter {ParameterName = "p3", Value = expectedClosed};
                Assert.That(p3.EDBDbType, Is.EqualTo(EDBDbType.Path));
                cmd.Parameters.Add(p1);
                cmd.Parameters.Add(p2);
                cmd.Parameters.Add(p3);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    reader.Read();

                    for (var i = 0; i < cmd.Parameters.Count; i++)
                    {
                        var expected = i == 0 ? expectedOpen : expectedClosed;
                        Assert.That(reader.GetFieldType(i), Is.EqualTo(typeof(EDBPath)));
                        var actual = reader.GetFieldValue<EDBPath>(i);
                        Assert.That(actual.Open, Is.EqualTo(expected.Open));
                        Assert.That(actual, Has.Count.EqualTo(expected.Count));
                        for (var j = 0; j < actual.Count; j++)
                            AssertPointsEqual(actual[j], expected[j]);
                    }
                }
            }
        }

        [Test]
        public async Task Polygon()
        {
            using (var conn = await OpenConnectionAsync())
            {
                var expected = new EDBPolygon(new EDBPoint(1, 2), new EDBPoint(3, 4));
                var cmd = new EDBCommand("SELECT @p1, @p2", conn);
                var p1 = new EDBParameter("p1", EDBDbType.Polygon) {Value = expected};
                var p2 = new EDBParameter {ParameterName = "p2", Value = expected};
                Assert.That(p2.EDBDbType, Is.EqualTo(EDBDbType.Polygon));
                cmd.Parameters.Add(p1);
                cmd.Parameters.Add(p2);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    reader.Read();

                    for (var i = 0; i < cmd.Parameters.Count; i++)
                    {
                        Assert.That(reader.GetFieldType(i), Is.EqualTo(typeof(EDBPolygon)));
                        var actual = reader.GetFieldValue<EDBPolygon>(i);
                        Assert.That(actual, Has.Count.EqualTo(expected.Count));
                        for (var j = 0; j < actual.Count; j++)
                            AssertPointsEqual(actual[j], expected[j]);
                    }
                }
            }
        }

        [Test]
        public async Task Circle()
        {
            using (var conn = await OpenConnectionAsync())
            {
                var expected = new EDBCircle(1, 2, 0.5);
                var cmd = new EDBCommand("SELECT @p1, @p2", conn);
                var p1 = new EDBParameter("p1", EDBDbType.Circle) {Value = expected};
                var p2 = new EDBParameter {ParameterName = "p2", Value = expected};
                Assert.That(p2.EDBDbType, Is.EqualTo(EDBDbType.Circle));
                cmd.Parameters.Add(p1);
                cmd.Parameters.Add(p2);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    reader.Read();

                    for (var i = 0; i < cmd.Parameters.Count; i++)
                    {
                        Assert.That(reader.GetFieldType(i), Is.EqualTo(typeof(EDBCircle)));
                        var actual = reader.GetFieldValue<EDBCircle>(i);
                        Assert.That(actual.X, Is.EqualTo(expected.X).Within(1).Ulps);
                        Assert.That(actual.Y, Is.EqualTo(expected.Y).Within(1).Ulps);
                        Assert.That(actual.Radius, Is.EqualTo(expected.Radius).Within(1).Ulps);
                    }
                }
            }
        }

        void AssertPointsEqual(EDBPoint actual, EDBPoint expected)
        {
            Assert.That(actual.X, Is.EqualTo(expected.X).Within(1).Ulps);
            Assert.That(actual.Y, Is.EqualTo(expected.Y).Within(1).Ulps);
        }

        public GeometricTypeTests(MultiplexingMode multiplexingMode) : base(multiplexingMode) {}
    }
}
