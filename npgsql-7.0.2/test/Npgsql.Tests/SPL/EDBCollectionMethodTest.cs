using System;
using NUnit.Framework;
using EnterpriseDB.EDBClient;
using System.Data;
using System.Data.SqlTypes;
using System.Xml.Linq;
using System.Collections;
using System.Threading;

#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

//EC-2575: Regression Tests for Collection methods in SPL

namespace EnterpriseDB.EDBClient.Tests.SPL
{
    internal class EDBCollectionMethodTest : TestBase
    {
        EDBConnection? conn = null;

        private static string[] DELETE_RESULT = {
            "COUNT: 5",
            "COUNT: 4",
            "Results: -100 -10 10 100 ",
            "COUNT: 2",
            "COUNT: 0" };
        private static string[] EXTEND_RESULT = {
            "COUNT: 5",
            "COUNT: 6",
            "Results: -100 -10 0 10 100 NULL "
            };
        private static string[] EXTEND_WITH_NUMBER_RESULT = {
            "COUNT: 5",
            "COUNT: 8",
            "Results: -100 -10 0 10 100 NULL NULL NULL "
            };
        private static string[] EXTEND_WITH_ELEMENT_RESULT = {
            "COUNT: 5",
            "COUNT: 8",
            "Results: -100 -10 0 10 100 -10 -10 -10 "
            };
        private static string[] TRIM_RESULT = {
            "COUNT: 5",
            "COUNT: 4"
            };
        private static string[] TRIM_WITH_NUMBER_RESULT = {
            "COUNT: 5",
            "COUNT: 3",
            "Results: -100 -10 0 "
            };

        [SetUp]
        public void Init()
        {
            conn = OpenConnection();

            Execute("DROP FUNCTION collection_count;");
            Execute("DROP PROCEDURE collection_exists;");
            Execute("DROP FUNCTION collection_first;");
            Execute("DROP FUNCTION collection_last;");
            Execute("DROP PROCEDURE collection_limit;");
            Execute("DROP FUNCTION collection_next;");
            Execute("DROP FUNCTION collection_prior;");

            //This example shows that an associative array can be sparsely populated, with gaps in the
            //sequence of assigned elements. COUNT includes only the elements that were assigned a
            //value.
            var countFun = "CREATE OR REPLACE FUNCTION collection_count()\n"
                            + "   RETURN INT \n"
                            + "IS \n"
                            + "DECLARE\n"
                            + "    TYPE sparse_arr_typ IS TABLE OF NUMBER INDEX BY BINARY_INTEGER;\n"
                            + "    sparse_arr      sparse_arr_typ;\n"
                            + "BEGIN\n"
                            + "    sparse_arr(-100)  := -100;\n"
                            + "    sparse_arr(-10)   := -10;\n"
                            + "    sparse_arr(0)     := 0;\n"
                            + "    sparse_arr(10)    := 10;\n"
                            + "    sparse_arr(100)   := 100;\n"
                            + "    return  sparse_arr.COUNT;\n"
                            + "END;";
            Execute(countFun);

            //The EXISTS method verifies that a subscript exists in a collection. EXISTS returns
            //TRUE if the subscript exists. If the subscript doesn't exist, EXISTS returns FALSE.
            //The method takes a single argument: the subscript that you are testing for.
            //The syntax is: <collection>.EXISTS(<subscript>)
            var existsPro = "CREATE OR REPLACE PROCEDURE collection_exists(\n"
                             + " exists10 OUT Boolean, \n"
                             + " exists20 OUT Boolean \n"
                             + ")\n"
                             + "IS\n"
                             + "DECLARE\n"
                             + "    TYPE sparse_arr_typ IS TABLE OF NUMBER INDEX BY BINARY_INTEGER;\n"
                             + "    sparse_arr      sparse_arr_typ;\n"
                             + "BEGIN\n"
                             + "    sparse_arr(-100)  := -100;\n"
                             + "    sparse_arr(-10)   := -10;\n"
                             + "    sparse_arr(0)     := 0;\n"
                             + "    sparse_arr(10)    := 10;\n"
                             + "    sparse_arr(100)   := 100;\n"
                             + "    exists10 := sparse_arr.exists(10); "
                             + "    exists20 := sparse_arr.exists(20); "
                             + "END;";
            Execute(existsPro);

            //FIRST is a method that returns the subscript of the first element in a collection.
            //The syntax for using FIRST is as follows: <collection>.FIRST
            //This example displays the first element of the associative array:
            var firstFun = "CREATE OR REPLACE FUNCTION collection_first()\n"
                            + "   RETURN INT \n"
                            + "IS \n"
                            + "DECLARE\n"
                            + "    TYPE sparse_arr_typ IS TABLE OF NUMBER INDEX BY BINARY_INTEGER;\n"
                            + "    sparse_arr      sparse_arr_typ;\n"
                            + "BEGIN\n"
                            + "    sparse_arr(-100)  := -100;\n"
                            + "    sparse_arr(-10)   := -10;\n"
                            + "    sparse_arr(0)     := 0;\n"
                            + "    sparse_arr(10)    := 10;\n"
                            + "    sparse_arr(100)   := 100;\n"
                            + "    return sparse_arr(sparse_arr.FIRST);\n"
                            + "END;";
            Execute(firstFun);

            //LAST is a method that returns the subscript of the last element in a collection.
            //The syntax for using LAST is as follows: <collection>.LAST
            //This example displays the last element of the associative array:
            var lastFun = "CREATE OR REPLACE FUNCTION collection_last()\n"
                           + "   RETURN INT \n"
                           + "IS \n"
                           + "DECLARE\n"
                           + "    TYPE sparse_arr_typ IS TABLE OF NUMBER INDEX BY BINARY_INTEGER;\n"
                           + "    sparse_arr      sparse_arr_typ;\n"
                           + "BEGIN\n"
                           + "    sparse_arr(-100)  := -100;\n"
                           + "    sparse_arr(-10)   := -10;\n"
                           + "    sparse_arr(0)     := 0;\n"
                           + "    sparse_arr(10)    := 10;\n"
                           + "    sparse_arr(100)   := 100;\n"
                           + "    return sparse_arr(sparse_arr.LAST);\n"
                           + "END;";
            Execute(lastFun);

            //LIMIT is a method that returns the maximum number of elements permitted in a collection.
            //LIMIT applies only to varrays. The syntax for using LIMIT is: <collection>.LIMIT
            var limitPro = "CREATE OR REPLACE PROCEDURE collection_limit(\n"
                            + " v_limit OUT Int, \n"
                            + " v_count OUT Int \n"
                            + ")\n"
                            + "IS\n"
                            + "DECLARE\n"
                            + "    TYPE dname_varray_typ IS VARRAY(4) OF VARCHAR2(14);\n"
                            + "    dname_varray    dname_varray_typ;\n"
                            + "BEGIN\n"
                            + "    dname_varray := dname_varray_typ(NULL, NULL, NULL);\n"
                            + "    v_limit := dname_varray.LIMIT; "
                            + "    v_count := dname_varray.COUNT; "
                            + "END;";
            Execute(limitPro);

            //NEXT is a method that returns the subscript that follows a specified subscript.
            //The method takes a single argument: the subscript that you are testing for.
            //The syntax is: <collection>.NEXT(<subscript>)
            //This example uses NEXT to return the subscript that follows subscript
            //10 in the associative array, sparse_arr:
            var nextFun = "CREATE OR REPLACE FUNCTION collection_next()\n"
                           + "   RETURN INT \n"
                           + "IS \n"
                           + "DECLARE\n"
                           + "    TYPE sparse_arr_typ IS TABLE OF NUMBER INDEX BY BINARY_INTEGER;\n"
                           + "    sparse_arr      sparse_arr_typ;\n"
                           + "BEGIN\n"
                           + "    sparse_arr(-100)  := -100;\n"
                           + "    sparse_arr(-10)   := -10;\n"
                           + "    sparse_arr(0)     := 0;\n"
                           + "    sparse_arr(10)    := 10;\n"
                           + "    sparse_arr(100)   := 100;\n"
                           + "    return sparse_arr.next(10);\n"
                           + "END;";
            Execute(nextFun);

            //The PRIOR method returns the subscript that precedes a specified subscript in a collection.
            //The method takes a single argument: the subscript that you are testing for. The syntax is:
            //<collection>.PRIOR(<subscript>)
            var priorFun = "CREATE OR REPLACE FUNCTION collection_prior()\n"
                            + "   RETURN INT \n"
                            + "IS \n"
                            + "DECLARE\n"
                            + "    TYPE sparse_arr_typ IS TABLE OF NUMBER INDEX BY BINARY_INTEGER;\n"
                            + "    sparse_arr      sparse_arr_typ;\n"
                            + "BEGIN\n"
                            + "    sparse_arr(-100)  := -100;\n"
                            + "    sparse_arr(-10)   := -10;\n"
                            + "    sparse_arr(0)     := 0;\n"
                            + "    sparse_arr(10)    := 10;\n"
                            + "    sparse_arr(100)   := 100;\n"
                            + "    return sparse_arr.prior(100);\n"
                            + "END;";
            Execute(priorFun);
        }

        [TearDown]
        public void Dispose()
        {
            TestUtil.closeDB(conn);
        }

        private int Execute(string query)
        {
            try
            {
                using (var com = new EDBCommand(query, conn))
                {
                    com.CommandType = CommandType.Text;
                    return com.ExecuteNonQuery();
                }
            }
            catch
            {
            }

            return 0;
        }

        [Test]
        public void CountTest()
        {
            var commandText = "collection_count()";

            using (var cstmt = new EDBCommand(commandText, conn))
            {
                cstmt.CommandType = CommandType.StoredProcedure;
                cstmt.Parameters.Add(new EDBParameter("ret", EDBTypes.EDBDbType.Integer, 10, "ret",
                    ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, null));

                cstmt.Prepare();
                cstmt.ExecuteNonQuery();

                var value = int.Parse(cstmt.Parameters[0].Value.ToString());

                Assert.AreEqual(5, value);
            }
        }

        [Test]
        public void ExistsTest()
        {
            var commandText = "collection_exists(:exists10, :exists20)";

            using (var cstmt = new EDBCommand(commandText, conn))
            {
                cstmt.CommandType = CommandType.StoredProcedure;
                cstmt.Parameters.Add(new EDBParameter("exists10", EDBTypes.EDBDbType.Boolean, 10, "exists10",
                    ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));

                cstmt.Parameters.Add(new EDBParameter("exists20", EDBTypes.EDBDbType.Boolean, 10, "exists20",
                    ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));

                cstmt.Prepare();
                cstmt.ExecuteNonQuery();

                var exists10 = cstmt.Parameters[0].Value.ToString().StartsWith('t') ? true : false;
                var exists20 = cstmt.Parameters[1].Value.ToString().StartsWith('t') ? true : false;

                Assert.AreEqual(true, exists10);
                Assert.AreEqual(false, exists20);
            }
        }

        [Test]
        public void FirstTest()
        {
            var commandText = "collection_first()";

            using (var cstmt = new EDBCommand(commandText, conn))
            {
                cstmt.CommandType = CommandType.StoredProcedure;
                cstmt.Parameters.Add(new EDBParameter("ret", EDBTypes.EDBDbType.Integer, 10, "ret",
                    ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, null));

                cstmt.Prepare();
                cstmt.ExecuteNonQuery();

                var value = int.Parse(cstmt.Parameters[0].Value.ToString());

                Assert.AreEqual(-100, value);
            }
        }

        [Test]
        public void LastTest()
        {
            var commandText = "collection_last()";

            using (var cstmt = new EDBCommand(commandText, conn))
            {
                cstmt.CommandType = CommandType.StoredProcedure;
                cstmt.Parameters.Add(new EDBParameter("ret", EDBTypes.EDBDbType.Integer, 10, "ret",
                    ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, null));

                cstmt.Prepare();
                cstmt.ExecuteNonQuery();

                var value = int.Parse(cstmt.Parameters[0].Value.ToString());

                Assert.AreEqual(100, value);
            }
        }

        [Test]
        public void LimitTest()
        {
            /*To verify this on PSQL, run the following:
             *  declare
                v_limit integer;
                v_count integer;
                begin
                collection_limit(v_limit, v_count);
                DBMS_OUTPUT.PUT_LINE('Limit : ' || v_limit);
                DBMS_OUTPUT.PUT_LINE('Count : ' || v_count);
                end;
             */
            var commandText = "collection_limit(:v_limit, :v_count)";

            using (var cstmt = new EDBCommand(commandText, conn))
            {
                cstmt.CommandType = CommandType.StoredProcedure;
                cstmt.Parameters.Add(new EDBParameter("v_limit", EDBTypes.EDBDbType.Integer, 10, "v_limit",
                    ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));

                cstmt.Parameters.Add(new EDBParameter("v_count", EDBTypes.EDBDbType.Integer, 10, "v_count",
                    ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));

                cstmt.Prepare();
                cstmt.ExecuteNonQuery();

                var exists10 = int.Parse(cstmt.Parameters[0].Value.ToString());
                var exists20 = int.Parse(cstmt.Parameters[1].Value.ToString());

                Assert.AreEqual(4, exists10);
                Assert.AreEqual(3, exists20);
            }
        }

        [Test]
        public void NextTest()
        {
            var commandText = "collection_next()";

            using (var cstmt = new EDBCommand(commandText, conn))
            {
                cstmt.CommandType = CommandType.StoredProcedure;
                cstmt.Parameters.Add(new EDBParameter("ret", EDBTypes.EDBDbType.Integer, 10, "ret",
                    ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, null));

                cstmt.Prepare();
                cstmt.ExecuteNonQuery();

                var value = int.Parse(cstmt.Parameters[0].Value.ToString());

                Assert.AreEqual(100, value);
            }
        }

        [Test]
        public void PriorTest()
        {
            var commandText = "collection_prior()";

            using (var cstmt = new EDBCommand(commandText, conn))
            {
                cstmt.CommandType = CommandType.StoredProcedure;
                cstmt.Parameters.Add(new EDBParameter("ret", EDBTypes.EDBDbType.Integer, 10, "ret",
                    ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, null));

                cstmt.Prepare();
                cstmt.ExecuteNonQuery();

                var value = int.Parse(cstmt.Parameters[0].Value.ToString());

                Assert.AreEqual(10, value);
            }
        }

        //JDBC has the following 6 tests in Anonymous blocks.
        //Since .NET has issues with anonymous blocks.
        //We have have implemented these tests using stored procedures.

        [Test]
        public void DeleteTest()
        {
            //Use this form of the DELETE method to remove all entries from a collection:
            //<collection>.DELETE
            //Use this form of the DELETE method to remove the specified entry from a collection:
            //<collection>.DELETE(<subscript>)
            //Use this form of the DELETE method to remove the entries that fall in the range
            //specified by first_subscript and last_subscript (including the entries for
            //the first_subscript and the last_subscript) from a collection:
            //<collection>.DELETE(<first_subscript>, <last_subscript>)
            Execute("DROP PROCEDURE Delete_SP;");

            var sqlStr = "CREATE OR REPLACE PROCEDURE Delete_SP()\n"
                      + " IS\n"
                      + " DECLARE\n"
                      + "    TYPE sparse_arr_typ IS TABLE OF NUMBER INDEX BY BINARY_INTEGER;\n"
                      + "    sparse_arr      sparse_arr_typ;\n"
                      + "    v_results       VARCHAR2(50);\n"
                      + "    v_sub           NUMBER;\n"
                      + "BEGIN\n"
                      + "    sparse_arr(-100)  := -100;\n"
                      + "    sparse_arr(-10)   := -10;\n"
                      + "    sparse_arr(0)     := 0;\n"
                      + "    sparse_arr(10)    := 10;\n"
                      + "    sparse_arr(100)   := 100;\n"
                      + "    DBMS_OUTPUT.PUT_LINE('COUNT: ' || sparse_arr.COUNT);\n"
                      + "    sparse_arr.DELETE(0);\n"
                      + "    DBMS_OUTPUT.PUT_LINE('COUNT: ' || sparse_arr.COUNT);\n"
                      + "    v_sub := sparse_arr.FIRST;\n"
                      + "    WHILE v_sub IS NOT NULL LOOP\n"
                      + "        IF sparse_arr(v_sub) IS NULL THEN\n"
                      + "            v_results := v_results || 'NULL ';\n"
                      + "        ELSE\n"
                      + "            v_results := v_results || sparse_arr(v_sub) || ' ';\n"
                      + "        END IF;\n"
                      + "        v_sub := sparse_arr.NEXT(v_sub);\n"
                      + "    END LOOP;\n"
                      + "    DBMS_OUTPUT.PUT_LINE('Results: ' || v_results);\n"
                      + " sparse_arr.DELETE(-10, 10);\n"
                      + "    DBMS_OUTPUT.PUT_LINE('COUNT: ' || sparse_arr.COUNT);\n"
                      + " sparse_arr.DELETE;\n  "
                      + "    DBMS_OUTPUT.PUT_LINE('COUNT: ' || sparse_arr.COUNT);\n"
                      + "END;";
            Execute(sqlStr);

            var mre = new ManualResetEvent(false);
            var notices = new ArrayList();
            NoticeEventHandler action = (sender, args) =>
            {
                notices.Add(args.Notice);
                mre.Set();
            };
            conn.Notice += action;
            try
            {
                using (var cstmt = new EDBCommand("Delete_SP()", conn))
                {
                    cstmt.CommandType = CommandType.StoredProcedure;

                    cstmt.Prepare();
                    cstmt.ExecuteNonQuery();
                }
                mre.WaitOne(5000);
                Assert.AreEqual(DELETE_RESULT.Length, notices.Count);
                for (var i = 0; i < notices.Count; i++)
                {
                    var notice = (PostgresNotice?)notices[i];
                    Assert.AreEqual(DELETE_RESULT[i], notice.MessageText);
                }
            }
            finally
            {
                conn.Notice -= action;
            }
            mre.Close();
        }

        [Test]
        public void ExtendTest()
        {
            //The EXTEND method increases the size of a collection. The EXTEND method has three variations.
            //The first variation appends a single NULL element to a collection. The syntax for
            //this variation is: <collection>.EXTEND
            Execute("DROP PROCEDURE Extend_SP;");

            var sqlStr = "CREATE OR REPLACE PROCEDURE Extend_SP()\n"
                      + " IS\n"
                      + " DECLARE\n"
                      + "    TYPE sparse_arr_typ IS TABLE OF NUMBER;\n"
                      + "    sparse_arr      sparse_arr_typ := sparse_arr_typ(-100,-10,0,10,100);\n"
                      + "    v_results       VARCHAR2(50);\n"
                      + "BEGIN\n"
                      + "    DBMS_OUTPUT.PUT_LINE('COUNT: ' || sparse_arr.COUNT);\n"
                      + "    sparse_arr.EXTEND;\n"
                      + "    DBMS_OUTPUT.PUT_LINE('COUNT: ' || sparse_arr.COUNT);\n"
                      + "    FOR i IN sparse_arr.FIRST .. sparse_arr.LAST LOOP\n"
                      + "        IF sparse_arr(i) IS NULL THEN\n"
                      + "            v_results := v_results || 'NULL ';\n"
                      + "        ELSE\n"
                      + "            v_results := v_results || sparse_arr(i) || ' ';\n"
                      + "        END IF;\n"
                      + "    END LOOP;\n"
                      + "    DBMS_OUTPUT.PUT_LINE('Results: ' || v_results);\n"
                      + "END;";

            Execute(sqlStr);

            var mre = new ManualResetEvent(false);
            var notices = new ArrayList();
            NoticeEventHandler action = (sender, args) =>
            {
                notices.Add(args.Notice);
                mre.Set();
            };
            conn.Notice += action;
            try
            {
                using (var cstmt = new EDBCommand("Extend_SP()", conn))
                {
                    cstmt.CommandType = CommandType.StoredProcedure;

                    cstmt.Prepare();
                    cstmt.ExecuteNonQuery();
                }
                mre.WaitOne(5000);
                Assert.AreEqual(EXTEND_RESULT.Length, notices.Count);
                for (var i = 0; i < notices.Count; i++)
                {
                    var notice = (PostgresNotice?)notices[i];
                    Assert.AreEqual(EXTEND_RESULT[i], notice.MessageText);
                }
            }
            finally
            {
                conn.Notice -= action;
            }
            mre.Close();
        }

        [Test]
        public void ExtendwithNumberTest()
        {
            //This variation of the EXTEND method appends a specified number of elements to
            //the end of a collection: <collection>.EXTEND(<count>)
            //This example uses the EXTEND method to append multiple null elements to a collection:
            Execute("DROP PROCEDURE ExtendwithNumber_SP;");

            var sqlStr = "CREATE OR REPLACE PROCEDURE ExtendwithNumber_SP()\n"
                      + " IS\n"
                      + " DECLARE\n"
                      + "    TYPE sparse_arr_typ IS TABLE OF NUMBER;\n"
                      + "    sparse_arr      sparse_arr_typ := sparse_arr_typ(-100,-10,0,10,100);\n"
                      + "    v_results       VARCHAR2(50);\n"
                      + "BEGIN\n"
                      + "    DBMS_OUTPUT.PUT_LINE('COUNT: ' || sparse_arr.COUNT);\n"
                      + "    sparse_arr.EXTEND(3);\n"
                      + "    DBMS_OUTPUT.PUT_LINE('COUNT: ' || sparse_arr.COUNT);\n"
                      + "    FOR i IN sparse_arr.FIRST .. sparse_arr.LAST LOOP\n"
                      + "        IF sparse_arr(i) IS NULL THEN\n"
                      + "            v_results := v_results || 'NULL ';\n"
                      + "        ELSE\n"
                      + "            v_results := v_results || sparse_arr(i) || ' ';\n"
                      + "        END IF;\n"
                      + "    END LOOP;\n" + "    DBMS_OUTPUT.PUT_LINE('Results: ' || v_results);\n"
                      + "END;";

            Execute(sqlStr);

            var mre = new ManualResetEvent(false);
            var notices = new ArrayList();
            NoticeEventHandler action = (sender, args) =>
            {
                notices.Add(args.Notice);
                mre.Set();
            };
            conn.Notice += action;
            try
            {
                using (var cstmt = new EDBCommand("ExtendwithNumber_SP()", conn))
                {
                    cstmt.CommandType = CommandType.StoredProcedure;

                    cstmt.Prepare();
                    cstmt.ExecuteNonQuery();
                }
                mre.WaitOne(5000);
                Assert.AreEqual(EXTEND_WITH_NUMBER_RESULT.Length, notices.Count);
                for (var i = 0; i < notices.Count; i++)
                {
                    var notice = (PostgresNotice?)notices[i];
                    Assert.AreEqual(EXTEND_WITH_NUMBER_RESULT[i], notice.MessageText);
                }
            }
            finally
            {
                conn.Notice -= action;
            }
            mre.Close();
        }

        [Test]
        public void ExtendWithElementTest()
        {
            //This variation of the EXTEND method appends a specified number of copies
            //of a particular element to the end of a collection:
            //<collection>.EXTEND(<count>, <index_number>)
            //This example uses the EXTEND method to append multiple copies of the second
            //element to the collection:
            Execute("DROP PROCEDURE ExtendWithElement_SP;");

            var sqlStr = "CREATE OR REPLACE PROCEDURE ExtendWithElement_SP()\n"
                      + " IS\n"
                      + " DECLARE\n"
            + "    TYPE sparse_arr_typ IS TABLE OF NUMBER;\n"
                + "    sparse_arr      sparse_arr_typ := sparse_arr_typ(-100,-10,0,10,100);\n"
                + "    v_results       VARCHAR2(50);\n"
                + "BEGIN\n"
                + "    DBMS_OUTPUT.PUT_LINE('COUNT: ' || sparse_arr.COUNT);\n"
                + "    sparse_arr.EXTEND(3, 2);\n"
                + "    DBMS_OUTPUT.PUT_LINE('COUNT: ' || sparse_arr.COUNT);\n"
                + "    FOR i IN sparse_arr.FIRST .. sparse_arr.LAST LOOP\n"
                + "        IF sparse_arr(i) IS NULL THEN\n"
                + "            v_results := v_results || 'NULL ';\n"
                + "        ELSE\n"
                + "            v_results := v_results || sparse_arr(i) || ' ';\n"
                + "        END IF;\n"
                + "    END LOOP;\n"
                + "    DBMS_OUTPUT.PUT_LINE('Results: ' || v_results);\n"
                + "END;\n";

            Execute(sqlStr);

            var mre = new ManualResetEvent(false);
            var notices = new ArrayList();
            NoticeEventHandler action = (sender, args) =>
            {
                notices.Add(args.Notice);
                mre.Set();
            };
            conn.Notice += action;
            try
            {
                using (var cstmt = new EDBCommand("ExtendWithElement_SP()", conn))
                {
                    cstmt.CommandType = CommandType.StoredProcedure;

                    cstmt.Prepare();
                    cstmt.ExecuteNonQuery();
                }
                mre.WaitOne(5000);
                Assert.AreEqual(EXTEND_WITH_ELEMENT_RESULT.Length, notices.Count);
                for (var i = 0; i < notices.Count; i++)
                {
                    var notice = (PostgresNotice?)notices[i];
                    Assert.AreEqual(EXTEND_WITH_ELEMENT_RESULT[i], notice.MessageText);
                }
            }
            finally
            {
                conn.Notice -= action;
            }
            mre.Close();
        }

        [Test]
        public void TrimTest()
        {
            //The TRIM method removes one or more elements from the end of a collection.
            //The syntax for the TRIM method is: <collection>.TRIM[(<count>)]
            //This example uses the TRIM method to remove an element from the end of a collection.
            Execute("DROP PROCEDURE Trim_SP;");

            var sqlStr = "CREATE OR REPLACE PROCEDURE Trim_SP()\n"
                      + " IS\n"
                      + " DECLARE\n"
                      + "    TYPE sparse_arr_typ IS TABLE OF NUMBER;\n"
                      + "    sparse_arr      sparse_arr_typ := sparse_arr_typ(-100,-10,0,10,100);\n"
                      + "BEGIN\n"
                      + "    DBMS_OUTPUT.PUT_LINE('COUNT: ' || sparse_arr.COUNT);\n"
                      + "    sparse_arr.TRIM;\n"
                      + "    DBMS_OUTPUT.PUT_LINE('COUNT: ' || sparse_arr.COUNT);\n"
                      + "END;\n";

            Execute(sqlStr);

            var mre = new ManualResetEvent(false);
            var notices = new ArrayList();
            NoticeEventHandler action = (sender, args) =>
            {
                notices.Add(args.Notice);
                mre.Set();
            };
            conn.Notice += action;
            try
            {
                using (var cstmt = new EDBCommand("Trim_SP()", conn))
                {
                    cstmt.CommandType = CommandType.StoredProcedure;

                    cstmt.Prepare();
                    cstmt.ExecuteNonQuery();
                }
                mre.WaitOne(5000);
                Assert.AreEqual(TRIM_RESULT.Length, notices.Count);
                for (var i = 0; i < notices.Count; i++)
                {
                    var notice = (PostgresNotice?)notices[i];
                    Assert.AreEqual(TRIM_RESULT[i], notice.MessageText);
                }
            }
            finally
            {
                conn.Notice -= action;
            }
            mre.Close();
        }

        [Test]
        public void TrimWithNumberTest()
        {
            //You can also specify the number of elements to remove from the end of the collection
            //using the TRIM method.
            Execute("DROP PROCEDURE TrimWithNumber_SP;");

            var sqlStr = "CREATE OR REPLACE PROCEDURE TrimWithNumber_SP()\n"
                      + " IS\n"
                      + " DECLARE\n"
                      + "    TYPE sparse_arr_typ IS TABLE OF NUMBER;\n"
                      + "    sparse_arr      sparse_arr_typ := sparse_arr_typ(-100,-10,0,10,100);\n"
                      + "    v_results       VARCHAR2(50);\n"
                      + "BEGIN\n"
                      + "    DBMS_OUTPUT.PUT_LINE('COUNT: ' || sparse_arr.COUNT);\n"
                      + "    sparse_arr.TRIM(2);\n"
                      + "    DBMS_OUTPUT.PUT_LINE('COUNT: ' || sparse_arr.COUNT);\n"
                      + "    FOR i IN sparse_arr.FIRST .. sparse_arr.LAST LOOP\n"
                      + "        IF sparse_arr(i) IS NULL THEN\n"
                      + "            v_results := v_results || 'NULL ';\n"
                      + "        ELSE\n"
                      + "            v_results := v_results || sparse_arr(i) || ' ';\n"
                      + "        END IF;\n"
                      + "    END LOOP;\n"
                      + "    DBMS_OUTPUT.PUT_LINE('Results: ' || v_results);\n"
                      + "END;\n";

            Execute(sqlStr);

            var mre = new ManualResetEvent(false);
            var notices = new ArrayList();
            NoticeEventHandler action = (sender, args) =>
            {
                notices.Add(args.Notice);
                mre.Set();
            };
            conn.Notice += action;
            try
            {
                using (var cstmt = new EDBCommand("TrimWithNumber_SP()", conn))
                {
                    cstmt.CommandType = CommandType.StoredProcedure;

                    cstmt.Prepare();
                    cstmt.ExecuteNonQuery();
                }
                mre.WaitOne(5000);
                Assert.AreEqual(TRIM_WITH_NUMBER_RESULT.Length, notices.Count);
                for (var i = 0; i < notices.Count; i++)
                {
                    var notice = (PostgresNotice?)notices[i];
                    Assert.AreEqual(TRIM_WITH_NUMBER_RESULT[i], notice.MessageText);
                }
            }
            finally
            {
                conn.Notice -= action;
            }
            mre.Close();
        }
    }
}
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8604 // Possible null reference argument.
