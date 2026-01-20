using System;
using NUnit.Framework;
using EnterpriseDB.EDBClient;
using System.Data;
using System.Data.SqlTypes;
using System.Xml.Linq;

#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

//EC-2589: Regression Tests for User-defined PL/SQL Subtypes

namespace EnterpriseDB.EDBClient.Tests.SPL;

[NonParallelizable]
internal class EDBUserDefinedSubtypeTest : EPASTestBase
{
    EDBConnection? conn = null;
    private static string ename = "SMITH";
    private static int empno = 7369;

    [SetUp]
    public void Init()
    {
        conn = OpenConnection();

        Execute("DROP PACKAGE BODY pkgUnconstrainedTest;");
        Execute("DROP PACKAGE pkgUnconstrainedTest;");
        Execute("DROP PACKAGE BODY pkgConstrainedTest;");
        Execute("DROP PACKAGE pkgConstrainedTest;");
        Execute("DROP PACKAGE BODY pkgSubtypeOfOtherSubtypeTest;");
        Execute("DROP PACKAGE pkgSubtypeOfOtherSubtypeTest;");
        Execute("DROP PACKAGE BODY pkgTypeOperatorTest;");
        Execute("DROP PACKAGE pkgTypeOperatorTest;");
        TestUtil.dropTable(conn, "emp1 CASCADE");

        Execute("CREATE TABLE emp1(empno NUMBER(4),  ename VARCHAR2(10))");

        var addCommand = "INSERT INTO emp1(empno,ename) VALUES(:empno, :ename)";

        using (var cstmt = new EDBCommand(addCommand, conn))
        {
            cstmt.CommandType = CommandType.Text;
            cstmt.Parameters.Add(new EDBParameter("empno", EDBTypes.EDBDbType.Numeric, 10, "empno",
                ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, empno));

            cstmt.Parameters.Add(new EDBParameter("ename", EDBTypes.EDBDbType.Varchar, 10, "ename",
                ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, ename));

            cstmt.Prepare();
            cstmt.ExecuteNonQuery();
        }
    }

    [TearDown]
    public void Dispose()
    {
        TestUtil.closeDB(conn);
        conn?.Dispose();
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
    public void UnconstrainedSubtypeTest()
    {
        //To create an unconstrained subtype, use the SUBTYPE command to
        //specify the new subtype name and the name of the type on which
        //the subtype is based. For example, the following command creates
        //a subtype named ename_typ that has all of the attributes
        //of the type varchar2;
        Execute("CREATE OR REPLACE PACKAGE pkgUnconstrainedTest Is \n "
                   + "  SUBTYPE ename_typ IS varchar2; \n"
                   + "  SUBTYPE empno_typ IS number; \n"
                   + "  Procedure unconstrainedSubtypeTest(emp_name Out ename_typ, emp_no Out empno_typ); \n"
                   + "End pkgUnconstrainedTest;");
        Execute("CREATE OR REPLACE PACKAGE BODY pkgUnconstrainedTest \n"
                      + "Is "
                      + "   Procedure unconstrainedSubtypeTest(emp_name Out ename_typ, emp_no Out empno_typ) \n"
                      + "   Is \n"
                      + "    BEGIN \n"
                      + "      SELECT ename, empno INTO emp_name, emp_no FROM emp1 WHERE empno = '7369'; \n"
                      + "    End unconstrainedSubtypeTest; \n"
                      + "End pkgUnconstrainedTest; \n");

        var commandText = "pkgUnconstrainedTest.unconstrainedSubtypeTest(:emp_name, :emp_no)";

        using (var cstmt = new EDBCommand(commandText, conn))
        {
            cstmt.CommandType = CommandType.StoredProcedure;

            cstmt.Parameters.Add(new EDBParameter("emp_name", EDBTypes.EDBDbType.Varchar, 10, "emp_name",
                ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null!));

            cstmt.Parameters.Add(new EDBParameter("emp_no", EDBTypes.EDBDbType.Numeric, 10, "emp_no",
                ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null!));

            cstmt.Prepare();
            cstmt.ExecuteNonQuery();

            var enameValue = cstmt.Parameters[0].Value.ToString();
            var empnoValue = double.Parse(cstmt.Parameters[1].Value.ToString());

            Assert.That(ename, Is.EqualTo(enameValue));
            Assert.That(empno, Is.EqualTo(empnoValue).Within(0.1));
        }
    }

    [Test]
    public void ConstrainedSubtypeTest()
    {
        //Include a length value when creating a subtype that's based on
        //a character type to define the maximum length of the subtype.
        //for example, ename_typ is  a VARCHAR data type but is
        //limited to 15 characters.
        Execute("CREATE OR REPLACE PACKAGE pkgConstrainedTest Is \n "
                    + "  SUBTYPE ename_typ IS varchar2(10); \n"
                    + "  Procedure constrainedSubtypeTest(emp_name Out ename_typ); \n"
                    + "End  pkgConstrainedTest;");
        Execute("CREATE OR REPLACE PACKAGE BODY  pkgConstrainedTest \n"
                    + "Is "
                    + "   Procedure constrainedSubtypeTest(emp_name Out ename_typ) \n"
                    + "   Is \n"
                    + "    BEGIN \n"
                    + "      SELECT ename INTO emp_name FROM emp1 WHERE empno = '7369'; \n"
                    + "    End constrainedSubtypeTest; \n"
                    + "End  pkgConstrainedTest; \n");

        var commandText = "pkgConstrainedTest.constrainedSubtypeTest(:emp_name)";

        using (var cstmt = new EDBCommand(commandText, conn))
        {
            cstmt.CommandType = CommandType.StoredProcedure;
            cstmt.Parameters.Add(new EDBParameter("emp_name", EDBTypes.EDBDbType.Varchar, 10, "emp_name",
                ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null!));

            cstmt.Prepare();
            cstmt.ExecuteNonQuery();

            var enameValue = cstmt.Parameters[0].Value.ToString();

            Assert.That(enameValue, Is.EqualTo(ename));
        }
    }

    [Test]
    public void SubtypeOfOtherSubtypeTest()
    {
        //You can also create a subtype (constrained or unconstrained)
        //that's a subtype of another subtype, for example, sub_name_typ is
        // a subtype of ename_typ
        Execute("CREATE OR REPLACE PACKAGE pkgSubtypeOfOtherSubtypeTest Is \n "
                    + "  SUBTYPE ename_typ IS varchar2(10); \n"
                    + "  SUBTYPE sub_ename_typ IS ename_typ; \n"
                    + "  Procedure subtypeOfOtherSubtypeTest(emp_name Out sub_ename_typ); \n"
                    + "End  pkgSubtypeOfOtherSubtypeTest;");
        Execute("CREATE OR REPLACE PACKAGE BODY  pkgSubtypeOfOtherSubtypeTest \n"
                    + "Is "
                    + "   Procedure subtypeOfOtherSubtypeTest(emp_name Out sub_ename_typ) \n"
                    + "   Is \n"
                    + "    BEGIN \n"
                    + "      SELECT ename INTO emp_name FROM emp1 WHERE empno = '7369'; \n"
                    + "    End subtypeOfOtherSubtypeTest; \n"
                    + "End  pkgSubtypeOfOtherSubtypeTest; \n");

        var commandText = "pkgSubtypeOfOtherSubtypeTest.subtypeOfOtherSubtypeTest(:emp_name)";

        using (var cstmt = new EDBCommand(commandText, conn))
        {
            cstmt.CommandType = CommandType.StoredProcedure;
            cstmt.Parameters.Add(new EDBParameter("emp_name", EDBTypes.EDBDbType.Varchar, 10, "emp_name",
                ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null!));

            cstmt.Prepare();
            cstmt.ExecuteNonQuery();

            var enameValue = cstmt.Parameters[0].Value.ToString();

            Assert.That(enameValue, Is.EqualTo(ename));
        }
    }

    [Test]
    public void TypeOperatorSubtypeTest()
    {
        //You can use %TYPE notation to declare a subtype anchored to a column
        //For example: ename_typ whose base type matches the type of the
        //empno column in the emp1 table
        Execute("CREATE OR REPLACE PACKAGE pkgTypeOperatorTest Is \n "
                + "  SUBTYPE ename_typ IS emp1.ename%TYPE; \n"
                + "  Procedure typeOperatorSubtypeTest(emp_name Out ename_typ); \n"
                + "End   pkgTypeOperatorTest;");
        Execute("CREATE OR REPLACE PACKAGE BODY   pkgTypeOperatorTest \n"
                + "Is "
                + "   Procedure typeOperatorSubtypeTest(emp_name Out ename_typ) \n"
                + "   Is \n"
                + "    BEGIN \n"
                + "      SELECT ename INTO emp_name FROM emp1 WHERE empno = '7369'; \n"
                + "    End typeOperatorSubtypeTest; \n"
                + "End   pkgTypeOperatorTest; \n");

        var commandText = "pkgTypeOperatorTest.typeOperatorSubtypeTest(:emp_name)";

        using (var cstmt = new EDBCommand(commandText, conn))
        {
            cstmt.CommandType = CommandType.StoredProcedure;
            cstmt.Parameters.Add(new EDBParameter("emp_name", EDBTypes.EDBDbType.Varchar, 10, "emp_name",
                ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null!));

            cstmt.Prepare();
            cstmt.ExecuteNonQuery();

            var enameValue = cstmt.Parameters[0].Value.ToString();

            Assert.That(enameValue, Is.EqualTo(ename));
        }
    }
}
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8604 // Possible null reference argument.

