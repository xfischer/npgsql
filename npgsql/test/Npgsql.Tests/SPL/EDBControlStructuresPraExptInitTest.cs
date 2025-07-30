using System;
using NUnit.Framework;
using System.Data;
using System.Threading;

//EC-2573: Regression Tests for Exception in SPL

namespace EnterpriseDB.EDBClient.Tests.SPL;

[NonParallelizable]
internal class EDBControlStructuresPraExptInitTest : EPASTestBase
{
    EDBConnection? conn = null;

    //This is not normal Setup method. We call it explicitly
    public void Init(string table, string func, string procPurchase, string procRecord, string pkgAr)
    {
        conn = OpenConnection();

        Execute("DROP PROCEDURE " + procRecord);
        Execute("DROP FUNCTION " + func);
        Execute("DROP PACKAGE BODY " + pkgAr);
        Execute("DROP PACKAGE " + pkgAr);
        Execute("DROP PROCEDURE " + procPurchase);

        Execute("DROP TABLE " + table + " CASCADE");

        Execute("CREATE TABLE " + table + "(cust_id NUMBER(8), cust_name VARCHAR2(20), balance NUMBER(10,2), "
                + "stmt_amount NUMBER(10,2))");

        var add1001 = "insert into " + table + "(cust_id, cust_name, balance, stmt_amount) "
                       + " values (1001, 'Mike', 20000,1000);";
        Execute(add1001);
        var add1002 = "insert into " + table + "(cust_id, cust_name, balance, stmt_amount) "
                       + " values (1002, 'Mike', 20000,1000);";
        Execute(add1002);

        //PRAGMA EXCEPTION_INIT associates a user-defined error code with an exception.
        //The following example uses a PRAGMA EXCEPTION_INIT declaration.
        var createPkg = "CREATE OR REPLACE PACKAGE " + pkgAr + " AS\n"
                         + "  overdrawn EXCEPTION;\n"
                         + "  PRAGMA EXCEPTION_INIT (overdrawn, -20100);"
                         + "  PROCEDURE check_balance(p_balance NUMBER, p_amount NUMBER);\n"
                         + "END;";
        Execute(createPkg);
        var pkgBody = "CREATE OR REPLACE PACKAGE BODY " + pkgAr + " AS\n"
                       + "  PROCEDURE check_balance(p_balance NUMBER, p_amount  NUMBER)\n"
                       + "  IS\n"
                       + "  BEGIN\n"
                       + "      IF (p_amount > p_balance) THEN\n"
                       + "        RAISE overdrawn;\n"
                       + "      END IF;\n"
                       + "   END;\n"
                       + " END;";
        Execute(pkgBody);

        //The following procedure calls the check_balance procedure. If p_amount is greater
        //than p_balance, check_balance raises an exception. The purchase procedure catches
        //the ar.overdrawn exception.
        var purPro = "CREATE OR REPLACE PROCEDURE " + procPurchase + "(customerID int, amount NUMERIC)\n"
                      + "AS\n"
                      + "  BEGIN\n"
                      + "     " + pkgAr + ".check_balance(" + func + "(customerid), amount);\n"
                      + "       " + procRecord + "(customerid, amount);\n"
                      + "  EXCEPTION\n"
                      + "     WHEN " + pkgAr + ".overdrawn THEN\n"
                      + "      DBMS_OUTPUT.PUT_LINE ('This account is overdrawn.');\n"
                      + "      DBMS_OUTPUT.PUT_LINE ('SQLCode :'||SQLCODE||' '||SQLERRM );\n"
                      + "END;";
        Execute(purPro);

        var getBalFun = "CREATE OR REPLACE function " + func + "(customerID INT) \n"
                         + "       RETURN NUMERIC\n"
                         + "AS\n"
                         + "    v_balance        NUMERIC ;\n"
                         + "BEGIN\n"
                         + "   SELECT balance INTO v_balance "
                         + "          FROM " + table + " WHERE cust_id = customerId;\n"
                         + "   return v_balance;\n"
                         + "EXCEPTION\n"
                         + "    WHEN NO_DATA_FOUND THEN\n"
                         + "      DBMS_OUTPUT.PUT_LINE('Customer # ' || customerId \n"
                         + "          || ' not found');\n"
                         + "END;";
        Execute(getBalFun);

        var recordPurPro = "CREATE OR REPLACE procedure " + procRecord + "(\n"
                            + "   customerID INT, amount  NUMERIC)\n"
                            + "AS \n"
                            + "BEGIN  \n"
                            + "    update " + table + " set balance = balance - amount, "
                            + "         stmt_amount = stmt_amount +amount \n"
                            + "         WHERE cust_id = customerId;\n"
                            + "    IF SQL%NOTFOUND THEN\n"
                            + "        DBMS_OUTPUT.PUT_LINE('Customer # ' || customerId || \n"
                             + "           ' not found');\n"
                            + "    END IF;\n"
                             + "END;\n";
        Execute(recordPurPro);

    }

    [TearDown]
    public void Dispose() => TestUtil.closeDB(conn);

    private void Execute(string query)
    {
        try
        {
            using var com = new EDBCommand(query, conn);
            com.CommandType = CommandType.Text;
            com.ExecuteNonQuery();
        }
        catch
        {
        }
    }

    private double GetCustomerStmtAmount(string table, int custId)
    {
        var command = "select stmt_amount from " + table + " where cust_id=" + custId;

        var selectCommand = new EDBCommand(command, conn);
        var selectResult = selectCommand.ExecuteReader();
        selectResult.Read();
        var amount = selectResult.GetDouble(0);
        selectResult.Close();

        return amount;
    }

    private double GetCustomerBalance(string table, int custId)
    {
        var command = "select balance from " + table + " where cust_id=" + custId;

        var selectCommand = new EDBCommand(command, conn);
        var selectResult = selectCommand.ExecuteReader();
        selectResult.Read();
        var balance = selectResult.GetDouble(0);
        selectResult.Close();

        return balance;
    }

    [Test]
    public void PurchaseTest()
    {
        Init("customer1", "getcustomerbalance1", "purchase1", "record_purchase1", "ar1");
        // call the purchase function with amount less than balance
        var commandText = "purchase1(:param1,:param2)";

        using (var cstmt = new EDBCommand(commandText, conn))
        {
            cstmt.CommandType = CommandType.StoredProcedure;

            cstmt.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Integer, 10, "param1",
                ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 1001));

            cstmt.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Numeric, 10, "param2",
                ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 3000));

            cstmt.Prepare();
            cstmt.ExecuteNonQuery();
        }

        Assert.AreEqual(4000, GetCustomerStmtAmount("customer1", 1001), 0.00);
        Assert.AreEqual(17000, GetCustomerBalance("customer1", 1001), 0.00);
    }

    [Test]
    public void PurchaseWithExceptionTest()
    {
        Init("customer2", "getcustomerbalance2", "purchase2", "record_purchase2", "ar2");
        // call the purchase function with amount more than balance
        // will raises overdrawn exception

        var commandText = "purchase2(:param1,:param2)";

        var mre = new ManualResetEvent(false);
        PostgresNotice? notice = null;
        NoticeEventHandler action = (sender, args) =>
        {
            Assert.IsNotNull(args.Notice);
            notice = args.Notice;
            mre.Set();
        };
        conn!.Notice += action;
        try
        {
            using (var cstmt = new EDBCommand(commandText, conn))
            {
                cstmt.CommandType = CommandType.StoredProcedure;

                cstmt.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Integer, 10, "param1",
                    ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 1002));

                cstmt.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Numeric, 10, "param2",
                    ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 30000));

                cstmt.Prepare();
                cstmt.ExecuteNonQuery();
            }

            mre.WaitOne(5000);
            Assert.IsNotNull(notice);
            Assert.AreEqual("SQLCode :-20100 User-Defined Exception", notice!.MessageText);
        }
        finally
        {
            conn.Notice -= action;
        }
    }
}
