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

//EC-2587: Regression Tests for Creating Subfunctions

namespace EnterpriseDB.EDBClient.Tests.SPL
{
    internal class EDBSubFunctionTest : TestBase
    {
        EDBConnection? conn = null;

        private static int[] numberValues = new int[] { 1, 2, 6, 24, 120 };
        private static int NUMBER_TOTAL = numberValues.Length;
        private static int FORWORD_DECLARE_RESULT = 5;
        private static string[] overloadMsgs = new string[] { "add_it BINARY_INTEGER",
            "add_it NUMBER", "add_it NUMBER","add_it REAL", "add_it DOUBLE PRECISION"};
        private static string[] overloadNumbers = new string[] { "75.0000",
            "75.6666", "75.6666","75.6666", "75.6666"};
        private static int MSG_TOTAL = overloadMsgs.Length;


        [SetUp]
        public void Init()
        {
            conn = OpenConnection();

            Execute("DROP function test_max_three();");

            //Subfunction test_max invokes subfunction add_one, which also invokes subfunction
            //test_max, so a forward declaration is required for one of the subprograms, which
            //is implemented for add_one at the beginning of the anonymous block declaration section.
            var forDecFun = "CREATE OR REPLACE FUNCTION test_max_three()"
                    + " RETURN NUMBER "
                    + " IS "
                    + "    FUNCTION add_one (\n"
                    + "        p_add       IN NUMBER\n"
                    + "    ) RETURN NUMBER;\n"
                    + "    FUNCTION test_max (\n"
                    + "        p_test      IN NUMBER)\n"
                    + "    RETURN NUMBER\n"
                    + "    IS\n"
                    + "    BEGIN\n"
                    + "        IF p_test < 5 THEN\n"
                    + "            RETURN add_one(p_test);\n"
                    + "        END IF;\n"
                    + "        RETURN p_test;\n"
                    + "    END;\n"
                    + "    FUNCTION add_one (\n"
                    + "        p_add       IN NUMBER)\n"
                    + "    RETURN NUMBER\n"
                    + "    IS\n"
                    + "    BEGIN\n"
                    + "        RETURN test_max(p_add + 1);\n"
                    + "    END;\n"
                    + "BEGIN\n"
                    + "    RETURN test_max(3);\n"
                    + "END;";
            Execute(forDecFun);
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
        public void SimpleFunctionTest()
        {
            //This test is implemented in JDBC as Anonymous block.
            //We have implemented it in a stored procedure because anonymous blocks do not work in .NET.

            //The following example shows the use of a recursive subfunction.
            Execute("DROP PROCEDURE SimpleFunction_SP;");

            var sqlStr = "CREATE OR REPLACE PROCEDURE SimpleFunction_SP()\n"
                + " IS\n"
                + " DECLARE\n"
                + "    FUNCTION factorial (\n"
                + "        n           BINARY_INTEGER\n"
                + "    ) RETURN BINARY_INTEGER\n"
                + "    IS\n"
                + "    BEGIN\n"
                + "        IF n = 1 THEN\n"
                + "            RETURN n;\n"
                + "        ELSE\n"
                + "            RETURN n * factorial(n-1);\n"
                + "        END IF;\n"
                + "    END factorial;\n"
                + "BEGIN\n"
                + "    FOR i IN 1..5 LOOP\n"
                + "        DBMS_OUTPUT.PUT_LINE(i || '! = ' || factorial(i));\n"
                + "    END LOOP;\n"
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
                using (var cstmt = new EDBCommand("SimpleFunction_SP", conn))
                {
                    cstmt.CommandType = CommandType.StoredProcedure;

                    cstmt.Prepare();
                    cstmt.ExecuteNonQuery();
                }
                mre.WaitOne(5000);
                Assert.AreEqual(NUMBER_TOTAL, notices.Count);
                for (var i = 0; i < NUMBER_TOTAL; i++)
                {
                    var notice = (PostgresNotice?)notices[i];
                    var result = notice.MessageText;
                    var arr = result.Split("=");
                    var value = arr[1].Trim();
                    Assert.AreEqual(numberValues[i].ToString(), value);
                }
            }
            finally
            {
                conn.Notice -= action;
            }
            mre.Close();
        }

        [Test]
        public void ForwardDeclarationsFunctionCallTest()
        {
            var commandText = "test_max_three()";

            var cstmt = new EDBCommand(commandText, conn);
            cstmt.CommandType = CommandType.StoredProcedure;

            cstmt.Parameters.Add(new EDBParameter("ret", EDBTypes.EDBDbType.Numeric, 10, "ret",
                ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, 31));

            cstmt.Prepare();
            cstmt.ExecuteNonQuery();
            var value = double.Parse(cstmt.Parameters[0].Value.ToString());
            Assert.AreEqual(FORWORD_DECLARE_RESULT, value, 0.1);
        }

        [Test]
        public void OverloadFunctionTest()
        {
            //Original JDBC test note.
            //The following example shows a group of overloaded subfunctions
            //invoked from within an anonymous block. The executable section
            //of the anonymous block contains the use of the CAST function
            //to invoke overloaded functions with certain data types.

            //This test is implemented in JDBC as Anonymous block.
            //We have implemented it in a stored procedure because anonymous blocks do not work in .NET.

            Execute("DROP PROCEDURE OverloadFunction_SP;");

            var sqlStr = "CREATE OR REPLACE PROCEDURE OverloadFunction_SP()\n"
                + " IS\n"
                + " DECLARE\n"
                + "    FUNCTION add_it (\n"
                + "        p_add_1     IN BINARY_INTEGER,\n"
                + "        p_add_2     IN BINARY_INTEGER\n"
                + "    ) RETURN VARCHAR2\n"
                + "    IS\n"
                + "    BEGIN\n"
                + "        RETURN 'add_it BINARY_INTEGER: ' || TO_CHAR(p_add_1 + p_add_2,9999.9999);\n"
                + "    END add_it;\n"
                + "    FUNCTION add_it (\n"
                + "        p_add_1     IN NUMBER,\n"
                + "        p_add_2     IN NUMBER\n"
                + "    ) RETURN VARCHAR2\n"
                + "    IS\n"
                + "    BEGIN\n"
                + "        RETURN 'add_it NUMBER: ' || TO_CHAR(p_add_1 + p_add_2,9999.9999);\n"
                + "    END add_it;\n"
                + "    FUNCTION add_it (\n"
                + "        p_add_1     IN REAL,\n"
                + "        p_add_2     IN REAL\n"
                + "    ) RETURN VARCHAR2\n"
                + "    IS\n"
                + "    BEGIN\n"
                + "        RETURN 'add_it REAL: ' || TO_CHAR(p_add_1 + p_add_2,9999.9999);\n"
                + "    END add_it;\n"
                + "    FUNCTION add_it (\n"
                + "        p_add_1     IN DOUBLE PRECISION,\n"
                + "        p_add_2     IN DOUBLE PRECISION\n"
                + "    ) RETURN VARCHAR2\n"
                + "    IS\n"
                + "    BEGIN\n"
                + "        RETURN 'add_it DOUBLE PRECISION: ' || TO_CHAR(p_add_1 + p_add_2,9999.9999);\n"
                + "    END add_it;\n"
                + "BEGIN\n"
                + "    DBMS_OUTPUT.PUT_LINE(add_it (25, 50));\n"
                + "    DBMS_OUTPUT.PUT_LINE(add_it (25.3333, 50.3333));\n"
                + "    DBMS_OUTPUT.PUT_LINE(add_it (TO_NUMBER(25.3333), TO_NUMBER(50.3333)));\n"
                + "    DBMS_OUTPUT.PUT_LINE(add_it (CAST('25.3333' AS REAL), CAST('50.3333' AS REAL)));\n"
                + "    DBMS_OUTPUT.PUT_LINE(add_it (CAST('25.3333' AS DOUBLE PRECISION),\n"
                + "        CAST('50.3333' AS DOUBLE PRECISION)));\n"
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
                using (var cstmt = new EDBCommand("OverloadFunction_SP", conn))
                {
                    cstmt.CommandType = CommandType.StoredProcedure;

                    cstmt.Prepare();
                    cstmt.ExecuteNonQuery();
                }
                mre.WaitOne(5000);
                Assert.AreEqual(MSG_TOTAL, notices.Count);
                for (var i = 0; i < notices.Count; i++)
                {
                    var notice = (PostgresNotice?)notices[i];
                    var value = notice.MessageText;
                    var arr = value.Split(":");
                    var msg = arr[0].Trim();
                    var number = arr[1].Trim();
                    Assert.AreEqual(overloadMsgs[i], msg);
                    Assert.AreEqual(overloadNumbers[i], number);
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

