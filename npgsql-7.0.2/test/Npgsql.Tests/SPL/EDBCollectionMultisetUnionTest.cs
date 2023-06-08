using System;
using NUnit.Framework;
using EnterpriseDB.EDBClient;
using System.Data;
using System.Data.SqlTypes;
using System.Xml.Linq;

#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

//EC-2574: Regression Tests for Working with Collections in SPL

namespace EnterpriseDB.EDBClient.Tests.SPL
{
    internal class EDBCollectionMultisetUnionTest : TestBase
    {
        EDBConnection? conn = null;

        private int[] MULTI_UNION_RESULT = { 10, 20, 30, 30, 40 };
        private int[] MULTI_UNION_DISTINCT_RESULT = { 10, 20, 30, 40 };
        private int[] MULTI_UNION_DISTINCT_RESULT02 = { 10, 20, 30, 40, 50 };

        [SetUp]
        public void Init()
        {
            conn = OpenConnection();
        }

        [TearDown]
        public void Dispose()
        {
            TestUtil.closeDB(conn);
        }

        private int Execute(string query, bool checkSuccess)
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
                if(checkSuccess)
                    Assert.Fail(ex.Message);
            }

            return 0;
        }

        [Test]
        [Ignore("EC-2650")]
        public void MultisetUnionTest()
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

            var cstmt = new EDBCommand(commandText, conn);
            cstmt.CommandType = CommandType.StoredProcedure;

            //cstmt.Parameters.Add(new EDBParameter("count", EDBTypes.EDBDbType.Integer, 10, "count",
            //    ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));

            //cstmt.Parameters.Add(new EDBParameter("collection", EDBTypes.EDBDbType.Array | EDBTypes.EDBDbType.Numeric, 10, "collection",
            //    ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));

            cstmt.DeriveParameters();

            cstmt.Prepare();
            cstmt.ExecuteNonQuery();

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
        [Ignore("EC-2650")]
        public void MultisetUnionDistinctTest()
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

            var cstmt = new EDBCommand(commandText, conn);
            cstmt.CommandType = CommandType.StoredProcedure;

            cstmt.DeriveParameters();

            cstmt.Prepare();
            cstmt.ExecuteNonQuery();

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
        [Ignore("EC-2650")]
        public void MultisetUnionDistinct02Test()
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

            var cstmt = new EDBCommand(commandText, conn);
            cstmt.CommandType = CommandType.StoredProcedure;

            cstmt.DeriveParameters();

            cstmt.Prepare();
            cstmt.ExecuteNonQuery();

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
}
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8604 // Possible null reference argument.
