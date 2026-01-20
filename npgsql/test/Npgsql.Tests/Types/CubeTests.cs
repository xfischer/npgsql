using System;
using System.Threading.Tasks;
using EnterpriseDB.EDBClient.Properties;
using EDBTypes;
using NUnit.Framework;

namespace EnterpriseDB.EDBClient.Tests.Types;

public class CubeTests : MultiplexingTestBase
{
    static readonly TestCaseData[] CubeValues =
    {
        new TestCaseData(new EDBCube(new[] { 1.0, 2.0, 3.0 }, new[] { 4.0, 5.0, 6.0 }), "(1, 2, 3),(4, 5, 6)")
            .SetName("Cube_MultiDimensional"),
        new TestCaseData(new EDBCube(new[] { 1.0, 2.0, 3.0 }), "(1, 2, 3)")
            .SetName("Cube_MultiDimensionalPoint"),
        new TestCaseData(new EDBCube(1.0), "(1)")
            .SetName("Cube_SingleDimensionalPoint"),
        new TestCaseData(new EDBCube(1.0, 2.0), "(1),(2)")
            .SetName("Cube_SingleDimensional")
    };

    [Test, TestCaseSource(nameof(CubeValues))]
    public Task Cube(EDBCube cube, string sqlLiteral)
        => AssertType(cube, sqlLiteral, "cube", EDBDbType.Cube, isDefault: true, isEDBDbTypeInferredFromClrType: false);

    [Test]
    public void Cube_Constructor_SingleValue()
    {
        var cube = new EDBCube(1.0);
        Assert.That(cube.IsPoint, Is.True);
        Assert.That(cube.Dimensions, Is.EqualTo(1));
        Assert.That(cube.LowerLeft, Is.EquivalentTo(new [] { 1.0 }));
        Assert.That(cube.UpperRight, Is.EquivalentTo(new [] { 1.0 }));
    }

    [Test]
    public void Cube_Constructor_SingleCoord_Point()
    {
        var cube = new EDBCube(1.0, 1.0);
        Assert.That(cube.IsPoint, Is.True);
        Assert.That(cube.Dimensions, Is.EqualTo(1));
        Assert.That(cube.LowerLeft, Is.EquivalentTo(new [] { 1.0 }));
        Assert.That(cube.UpperRight, Is.EquivalentTo(new [] { 1.0 }));
    }

    [Test]
    public void Cube_Constructor_SingleCoord_NotPoint()
    {
        var cube = new EDBCube(1.0, 2.0);
        Assert.That(cube.IsPoint, Is.False);
        Assert.That(cube.Dimensions, Is.EqualTo(1));
        Assert.That(cube.LowerLeft, Is.EquivalentTo(new [] { 1.0 }));
        Assert.That(cube.UpperRight, Is.EquivalentTo(new [] { 2.0 }));
    }

    [Test]
    public void Cube_Constructor_LowerLeft_UpperRight_NotPoint()
    {
        var cube = new EDBCube(new[] { 1.0, 2.0 }, new[] { 3.0, 4.0 });
        Assert.That(cube.IsPoint, Is.False);
        Assert.That(cube.Dimensions, Is.EqualTo(2));
        Assert.That(cube.LowerLeft, Is.EquivalentTo(new [] { 1.0, 2.0 }));
        Assert.That(cube.UpperRight, Is.EquivalentTo(new [] { 3.0, 4.0 }));
    }

    [Test]
    public void Cube_Constructor_LowerLeft_UpperRight_Point()
    {
        var cube = new EDBCube(new[] { 1.0, 2.0 }, new[] { 1.0, 2.0 });
        Assert.That(cube.IsPoint, Is.True);
        Assert.That(cube.Dimensions, Is.EqualTo(2));
        Assert.That(cube.LowerLeft, Is.EquivalentTo(new [] { 1.0, 2.0 }));
        Assert.That(cube.UpperRight, Is.EquivalentTo(new [] { 1.0, 2.0 }));
    }

    [Test]
    public void Cube_Constructor_AddDimension_Single_Point()
    {
        var existingCube = new EDBCube(new[] { 1.0, 2.0, 3.0 });
        var cube = new EDBCube(existingCube, 4.0);
        Assert.That(cube.IsPoint, Is.True);
        Assert.That(cube.Dimensions, Is.EqualTo(4));
        Assert.That(cube.LowerLeft, Is.EquivalentTo(new [] { 1.0, 2.0, 3.0, 4.0 }));
        Assert.That(cube.UpperRight, Is.EquivalentTo(new [] { 1.0, 2.0, 3.0, 4.0 }));
    }

    [Test]
    public void Cube_Constructor_AddDimension_Single_NotPoint()
    {
        var existingCube = new EDBCube(new [] { 1.0, 2.0 }, new [] { 3.0, 4.0 });
        var cube = new EDBCube(existingCube, 3.0);
        Assert.That(cube.IsPoint, Is.False);
        Assert.That(cube.Dimensions, Is.EqualTo(3));
        Assert.That(cube.LowerLeft, Is.EquivalentTo(new [] { 1.0, 2.0, 3.0 }));
        Assert.That(cube.UpperRight, Is.EquivalentTo(new [] { 3.0, 4.0, 3.0 }));
    }

    [Test]
    public void Cube_Constructor_AddDimension_LowerLeft_UpperRight_Point()
    {
        var existingCube = new EDBCube(new[] { 1.0, 2.0, 3.0 });
        var cube = new EDBCube(existingCube, 4.0, 4.0);
        Assert.That(cube.IsPoint, Is.True);
        Assert.That(cube.Dimensions, Is.EqualTo(4));
        Assert.That(cube.LowerLeft, Is.EquivalentTo(new [] { 1.0, 2.0, 3.0, 4.0 }));
        Assert.That(cube.UpperRight, Is.EquivalentTo(new [] { 1.0, 2.0, 3.0, 4.0 }));
    }

    [Test]
    public void Cube_Constructor_AddDimension_LowerLeft_UpperRight_NotPoint()
    {
        var existingCube = new EDBCube(new [] { 1.0, 2.0 }, new [] { 3.0, 4.0 });
        var cube = new EDBCube(existingCube, 4.0, 5.0);
        Assert.That(cube.IsPoint, Is.False);
        Assert.That(cube.Dimensions, Is.EqualTo(3));
        Assert.That(cube.LowerLeft, Is.EquivalentTo(new [] { 1.0, 2.0, 4.0 }));
        Assert.That(cube.UpperRight, Is.EquivalentTo(new [] { 3.0, 4.0, 5.0 }));
    }

    [Test]
    public void Cube_Subset()
    {
        var cube = new EDBCube(new [] { 1.0, 2.0, 3.0 }, new [] { 4.0, 5.0, 6.0 });
        Assert.That(cube.ToSubset(0, 2, 1, 1), Is.EqualTo(new EDBCube(new [] { 1.0, 3.0, 2.0, 2.0 }, new [] { 4.0, 6.0, 5.0, 5.0 })));
    }

    [Test]
    public void Cube_ToString_NotPoint()
    {
        var cube = new EDBCube(new[] { 1.0, 2.0, 3.0 }, new[] { 4.0, 5.0, 6.0 });
        Assert.That(cube.ToString(), Is.EqualTo("(1, 2, 3),(4, 5, 6)"));
    }

    [Test]
    public void Cube_ToString_Point()
    {
        var cube = new EDBCube(new[] { 1.0, 2.0, 3.0 });
        Assert.That(cube.ToString(), Is.EqualTo("(1, 2, 3)"));
    }

    [Test]
    public async Task Cube_Array()
    {
        var data = new[]
        {
            new EDBCube(new[] { 1.0, 2.0 }, new[] { 3.0, 4.0 }),
            new EDBCube(new[] { 5.0, 6.0 }),
            new EDBCube(1.0, 2.0)
        };

        await AssertType(
            data,
            @"{""(1, 2),(3, 4)"",""(5, 6)"",""(1),(2)""}",
            "cube[]",
            EDBDbType.Cube | EDBDbType.Array,
            isDefault: true,
            isEDBDbTypeInferredFromClrType: false);
    }

    [Test]
    public void Cube_DimensionMismatch_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() => new EDBCube(new[] { 1.0, 2.0 }, new[] { 3.0 }));
        Assert.That(ex!.Message, Does.Contain("Different point dimensions"));
    }

    [Test]
    public Task Cube_NegativeValues()
        => AssertType(
            new EDBCube(new[] { -1.0, -2.0, -3.0 }, new[] { -4.0, -5.0, -6.0 }),
            "(-1, -2, -3),(-4, -5, -6)",
            "cube",
            EDBDbType.Cube,
            isDefault: true,
            isEDBDbTypeInferredFromClrType: false);

    [Test]
    public void Cube_Equality_HashCode()
    {
        var cube1 = new EDBCube(new[] { 1.0, 2.0 }, new[] { 3.0, 4.0 });
        var cube2 = new EDBCube(new[] { 1.0, 2.0 }, new[] { 3.0, 4.0 });
        var cube3 = new EDBCube(new[] { 1.0, 2.0 }, new[] { 3.0, 5.0 });

        // Test equality
        Assert.That(cube1, Is.EqualTo(cube2));
        Assert.That(cube1 == cube2, Is.True);
        Assert.That(cube1 != cube3, Is.True);
        Assert.That(cube1.Equals(cube2), Is.True);
        Assert.That(cube1.Equals(cube3), Is.False);

        // Test hash code consistency
        Assert.That(cube1.GetHashCode(), Is.EqualTo(cube2.GetHashCode()));
        Assert.That(cube1.GetHashCode(), Is.Not.EqualTo(cube3.GetHashCode()));
    }

    [Test]
    public Task Cube_ZeroValues()
        => AssertType(
            new EDBCube(0.0, 0.0),
            "(0)",
            "cube",
            EDBDbType.Cube,
            isDefault: true,
            isEDBDbTypeInferredFromClrType: false);

    [Test]
    public Task Cube_MaxDimensions()
    {
        var lowerLeft = new double[100];
        var upperRight = new double[100];
        for (var i = 0; i < 100; i++)
        {
            lowerLeft[i] = i;
            upperRight[i] = i + 100;
        }

        var expectedLower = string.Join(", ", lowerLeft);
        var expectedUpper = string.Join(", ", upperRight);
        var expected = $"({expectedLower}),({expectedUpper})";

        return AssertType(
            new EDBCube(lowerLeft, upperRight),
            expected,
            "cube",
            EDBDbType.Cube,
            isDefault: true,
            isEDBDbTypeInferredFromClrType: false);
    }

    [Test]
    public async Task Cube_not_supported_by_default_on_EDBSlimSourceBuilder()
    {
        var errorMessage = string.Format(
            EDBStrings.CubeNotEnabled, nameof(EDBSlimDataSourceBuilder.EnableCube), nameof(EDBSlimDataSourceBuilder));

        var dataSourceBuilder = new EDBSlimDataSourceBuilder(ConnectionString);
        await using var dataSource = dataSourceBuilder.Build();

        var exception =
            await AssertTypeUnsupportedRead<EDBCube>("(1),(2)", "cube", dataSource);
        Assert.That(exception.InnerException!.Message, Is.EqualTo(errorMessage));
        exception = await AssertTypeUnsupportedWrite<EDBCube>(new EDBCube(1.0, 2.0), "cube", dataSource);
        Assert.That(exception.InnerException!.Message, Is.EqualTo(errorMessage));
    }

    [Test]
    public async Task EDBSlimSourceBuilder_EnableCube()
    {
        var dataSourceBuilder = new EDBSlimDataSourceBuilder(ConnectionString);
        dataSourceBuilder.EnableCube();
        await using var dataSource = dataSourceBuilder.Build();

        await AssertType(dataSource, new EDBCube(1.0, 2.0), "(1),(2)", "cube", EDBDbType.Cube, isDefaultForWriting: false, skipArrayCheck: true);
    }

    [Test]
    public async Task EDBSlimSourceBuilder_EnableArrays()
    {
        var dataSourceBuilder = new EDBSlimDataSourceBuilder(ConnectionString);
        dataSourceBuilder.EnableCube();
        dataSourceBuilder.EnableArrays();
        await using var dataSource = dataSourceBuilder.Build();

        await AssertType(dataSource, new EDBCube(1.0, 2.0), "(1),(2)", "cube", EDBDbType.Cube, isDefaultForWriting: false);
    }

    [OneTimeSetUp]
    public async Task SetUp()
    {
        await using var conn = await OpenConnectionAsync();
        TestUtil.MinimumPgVersion(conn, "13.0");
        await TestUtil.EnsureExtensionAsync(conn, "cube");
    }

    public CubeTests(MultiplexingMode multiplexingMode) : base(multiplexingMode) { }
}
