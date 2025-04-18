using System;
using NUnit.Framework;
using System.Data;
using System.Threading.Tasks;
using System.Collections.Generic;

#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

//EC-2574: Regression Tests for Working with Collections in SPL

namespace EnterpriseDB.EDBClient.Tests.SPL;

internal class EDBCollectionMultisetUnionTest : EPASTestBase
{
    private readonly int[] MULTI_UNION_RESULT = { 10, 20, 30, 30, 40 };
    private readonly int[] MULTI_UNION_DISTINCT_RESULT = { 10, 20, 30, 40 };
    private readonly int[] MULTI_UNION_DISTINCT_RESULT02 = { 10, 20, 30, 40, 50 };



    private int Execute(string query, bool checkSuccess)
    {
        var dataSourceBuilder = new EDBDataSourceBuilder(ConnectionString);
        using var dataSource = dataSourceBuilder.Build();
        using (var conn = dataSource.OpenConnection())
            return Execute(query, conn, checkSuccess);
    }

    private static int Execute(string query, EDBConnection conn, bool checkSuccess)
    {
        try
        {
            using (var com = new EDBCommand(query, conn))
            {
                com.CommandType = CommandType.Text;
                return com.ExecuteNonQuery();
            }
        }
        catch (Exception ex)
        {
            if (checkSuccess)
                Assert.Fail(ex.Message);
        }

        return 0;
    }

    [Test]
    [NonParallelizable]
    public void MultisetUnionTest([Values] bool deriveParameters)
    {
        Execute("DROP PACKAGE BODY mulUnPkg;", false);
        Execute("DROP PACKAGE mulUnPkg;", false);

        //The MULTISET UNION operator combines two collections to
        //form a third collection. The signature is:
        //<coll_1> MULTISET UNION [ ALL | DISTINCT | UNIQUE ] <coll_2>
        //This example uses the MULTISET UNION operator to combine
        //collection_1 and collection_2 into a third collection, collection_3:
        var mulUnPkg = "CREATE OR REPLACE PACKAGE mulUnPkg \n"
                        + "IS \n"
                        + "   TYPE int_arr_typ IS TABLE OF NUMBER(2);\n"
                        + "   PROCEDURE mulUnionTest( \n"
                        + "      count         OUT INTEGER, \n"
                        + "      collection_3  OUT int_arr_typ); \n"
                        + "END mulUnPkg; ";
        Execute(mulUnPkg, true);

        var mulUnPkgBody = "CREATE OR REPLACE PACKAGE BODY mulUnPkg \n"
                            + "Is \n"
                            + "  PROCEDURE mulUnionTest(\n"
                            + "     count         OUT integer, \n "
                            + "     collection_3  OUT int_arr_typ)\n"
                            + "  IS\n  "
                            + "  DECLARE\n"
                            + "    collection_1    int_arr_typ;\n"
                            + "    collection_2    int_arr_typ;\n"
                            + "  BEGIN\n"
                            + "    collection_1 := int_arr_typ(10,20,30);\n"
                            + "    collection_2 := int_arr_typ(30,40);\n"
                            + "    collection_3 := collection_1 MULTISET UNION ALL collection_2;\n"
                            + "    count :=  collection_3.COUNT;\n"
                            + "  END mulUnionTest;\n"
                            + "End mulUnPkg;";
        Execute(mulUnPkgBody, true);


        var commandText = "mulUnPkg.mulUnionTest";//(:count, :collection)

        var dataSourceBuilder = new EDBDataSourceBuilder(ConnectionString);
        var dataSource = dataSourceBuilder.Build();
        using var conn = dataSource.OpenConnection();
        var cstmt = new EDBCommand(commandText, conn)
        {
            CommandType = CommandType.StoredProcedure
        };


        if (deriveParameters)
        {
            cstmt.DeriveParameters();
        }
        else
        {
            cstmt.Parameters.Add(new EDBParameter()
            {
                Direction = ParameterDirection.Output,
                DataTypeName = "integer"
            });
            cstmt.Parameters.Add(new EDBParameter()
            {
                Direction = ParameterDirection.Output,
                DataTypeName = "mulunpkg.int_arr_typ"
            });
        }

        cstmt.Prepare();
        cstmt.ExecuteNonQuery();

        Assert.AreEqual(2, cstmt.Parameters.Count);
        Assert.AreEqual("integer", cstmt.Parameters[0].DataTypeName);
        Assert.AreEqual("mulunpkg.int_arr_typ", cstmt.Parameters[1].DataTypeName);

        var count = (int)cstmt.Parameters[0].Value!;
        Assert.AreEqual(5, count);
        var arr = (List<object>)cstmt.Parameters[1].Value!;
        Assert.AreEqual(5, arr.Count);
        for (var i = 0; i < arr.Count; i++)
        {
            var value = (decimal)arr[i];
            Assert.AreEqual(MULTI_UNION_RESULT[i], (int)value);
        }

        //The following code is from JDBC test and should be converted to .NET
        //if and when this issue is resolved.

        //int count = cstmt.getInt(1);
        //Assert.assertEquals(5, count);
        //Array arr = (Array)cstmt.getObject(2);
        //BigDecimal items[] = (BigDecimal[])arr.getArray();
        //Assert.assertEquals(5, items.length);
        //for (int i = 0; i<items.length; i++) {
        //    BigDecimal value = items[i];
        //Assert.assertEquals(MULTI_UNION_RESULT[i], value.intValue());
        //}
    }

    [Test]
    [NonParallelizable]
    //[Ignore("EC-2650")]
    public async Task MultisetUnionDistinctTest([Values] bool deriveParameters)
    {
        Execute("DROP PACKAGE BODY mulUnDisPkg;", false);
        Execute("DROP PACKAGE mulUnDisPkg;", false);

        //The resulting collection includes one entry for each element
        //in collection_1 and collection_2. If you use the DISTINCT keyword.
        var mulUnDisPkg = " CREATE OR REPLACE PACKAGE mulUnDisPkg \n"
                           + " Is \n"
                           + "   TYPE int_arr_typ IS TABLE OF NUMBER(2);\n"
                           + "   PROCEDURE mulUnionDistinctTest(\n"
                           + "      count         OUT INTEGER, \n"
                           + "      collection_3  OUT int_arr_typ); \n"
                           + " END mulUnDisPkg; ";
        Execute(mulUnDisPkg, true);

        var mulUnDisPkgBody = "CREATE OR REPLACE PACKAGE BODY mulUnDisPkg \n"
                               + "  Is \n"
                               + "  PROCEDURE mulUnionDistinctTest(\n"
                               + "      count OUT INTEGER, \n "
                               + "      collection_3  OUT int_arr_typ)\n"
                               + "  IS  \n"
                               + "  DECLARE\n"
                               + "    collection_1    int_arr_typ;\n"
                               + "    collection_2    int_arr_typ;\n"
                               + "  BEGIN\n"
                               + "    collection_1 := int_arr_typ(10,20,30);\n"
                               + "    collection_2 := int_arr_typ(30,40);\n"
                               + "    collection_3 := collection_1 MULTISET UNION DISTINCT collection_2;"
                               + "    count :=  collection_3.COUNT;\n"
                               + "  END mulUnionDistinctTest;\n"
                               + "END mulUnDisPkg;";
        Execute(mulUnDisPkgBody, true);

        var commandText = "mulUnDisPkg.mulUnionDistinctTest";

        var dataSourceBuilder = new EDBDataSourceBuilder(ConnectionString);
        await using var ds = dataSourceBuilder.Build();
        await using var conn = await ds.OpenConnectionAsync();

        var cstmt = new EDBCommand(commandText, conn)
        {
            CommandType = CommandType.StoredProcedure
        };

        if (deriveParameters)
        {
            cstmt.DeriveParameters();
        }
        else
        {
            cstmt.Parameters.Add(new EDBParameter()
            {
                Direction = ParameterDirection.Output,
                DataTypeName = "integer"
            });
            cstmt.Parameters.Add(new EDBParameter()
            {
                Direction = ParameterDirection.Output,
                DataTypeName = "mulundispkg.int_arr_typ"
            });
        }

        await cstmt.PrepareAsync();
        await cstmt.ExecuteNonQueryAsync();

        Assert.AreEqual(2, cstmt.Parameters.Count);
        Assert.AreEqual("integer", cstmt.Parameters[0].DataTypeName);
        Assert.AreEqual("mulundispkg.int_arr_typ", cstmt.Parameters[1].DataTypeName);

        var count = (int)cstmt.Parameters[0].Value!;
        Assert.AreEqual(4, count);
        var  arr = (List<object>)cstmt.Parameters[1].Value!;
        Assert.AreEqual(4, arr.Count);
        for(var i = 0; i < arr.Count; i++)
        {
            var value = (decimal)arr[i];
            Assert.AreEqual(MULTI_UNION_DISTINCT_RESULT[i], (int)value);
        }

        //The following code is from JDBC test and should be converted to .NET
        //if and when this issue is resolved.

        //int count = cstmt.getInt(1);
        //Assert.assertEquals(4, count);
        //Array arr = (Array)cstmt.getObject(2);
        //BigDecimal items[] = (BigDecimal[])arr.getArray();
        //Assert.assertEquals(4, items.length);
        //for (int i = 0; i<items.length; i++) {
        //    BigDecimal value = items[i];
        //Assert.assertEquals(MULTI_UNION_DISTINCT_RESULT[i], value.intValue());
        //}
        //cstmt.close();
    }

    [Test]
    [NonParallelizable]
    public void MultisetUnionDistinct02Test([Values] bool deriveParameters)
    {
        Execute("DROP PACKAGE BODY mulUnDisPkg02;", false);
        Execute("DROP PACKAGE mulUnDisPkg02;", false);

        //In this example, the MULTISET UNION DISTINCT operator removes
        //duplicate entries that are stored in the same collection:
        var mulUnDisPkg02 = "CREATE OR REPLACE PACKAGE mulUnDisPkg02 "
                             + "IS \n"
                             + "   TYPE int_arr_typ IS TABLE OF NUMBER(2);\n"
                             + "   Procedure mulUnionDistinctTest02(\n"
                             + "      count         OUT INTEGER, "
                             + "      collection_3  OUT int_arr_typ); \n"
                             + "End mulUnDisPkg02; ";
        Execute(mulUnDisPkg02, true);

        var mulUnDisPkgBody02 = "CREATE OR REPLACE PACKAGE BODY mulUnDisPkg02 \n"
                                 + "IS\n"
                                 + "  Procedure mulUnionDistinctTest02("
                                 + "     count         OUT INTEGER, \n "
                                 + "     collection_3  OUT int_arr_typ) \n"
                                 + "  IS "
                                 + "  DECLARE\n"
                                 + "    collection_1    int_arr_typ;\n"
                                 + "    collection_2    int_arr_typ;\n"
                                 + "  BEGIN\n"
                                 + "    collection_1 := int_arr_typ(10,20,30,30);\n"
                                 + "    collection_2 := int_arr_typ(40,50);\n"
                                 + "    collection_3 := collection_1 MULTISET UNION DISTINCT collection_2;"
                                 + "    count :=  collection_3.COUNT;\n"
                                 + "  END mulUnionDistinctTest02;\n"
                                 + "END mulUnDisPkg02;";
        Execute(mulUnDisPkgBody02, true);

        var commandText = "mulUnDisPkg02.mulUnionDistinctTest02";

        var dataSourceBuilder = new EDBDataSourceBuilder(ConnectionString);
        var dataSource = dataSourceBuilder.Build();
        using var conn = dataSource.OpenConnection();
        var cstmt = new EDBCommand(commandText, conn)
        {
            CommandType = CommandType.StoredProcedure
        };

        if (deriveParameters)
        {
            cstmt.DeriveParameters();
        }
        else
        {
            cstmt.Parameters.Add(new EDBParameter()
            {
                Direction = ParameterDirection.Output,
                DataTypeName = "integer"
            });
            cstmt.Parameters.Add(new EDBParameter()
            {
                Direction = ParameterDirection.Output,
                DataTypeName = "mulundispkg02.int_arr_typ"
            });
        }

        cstmt.Prepare();
        cstmt.ExecuteNonQuery();

        Assert.AreEqual(2, cstmt.Parameters.Count);
        Assert.AreEqual("integer", cstmt.Parameters[0].DataTypeName);
        Assert.AreEqual("mulundispkg02.int_arr_typ", cstmt.Parameters[1].DataTypeName);

        var count = (int)cstmt.Parameters[0].Value!;
        Assert.AreEqual(5, count);
        var arr = (List<object>)cstmt.Parameters[1].Value!;
        Assert.AreEqual(5, arr.Count);
        for (var i = 0; i < arr.Count; i++)
        {
            var value = (decimal)arr[i];
            Assert.AreEqual(MULTI_UNION_DISTINCT_RESULT02[i], (int)value);
        }

        //The following code is from JDBC test and should be converted to .NET
        //if and when this issue is resolved.

        //int count = cstmt.getInt(1);
        //Assert.assertEquals(5, count);
        //Array arr = (Array)cstmt.getObject(2);
        //BigDecimal items[] = (BigDecimal[])arr.getArray();
        //Assert.assertEquals(5, items.length);
        //for (int i = 0; i<items.length; i++) {
        //    BigDecimal value = items[i];
        //Assert.assertEquals(MULTI_UNION_DISTINCT_RESULT02[i], value.intValue());
        //}
        //cstmt.close();
    }
}
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8604 // Possible null reference argument.
