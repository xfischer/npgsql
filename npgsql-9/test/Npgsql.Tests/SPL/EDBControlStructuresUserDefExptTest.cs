using System;
using NUnit.Framework;
using EnterpriseDB.EDBClient;
using System.Data;
using System.Data.SqlTypes;
using System.Xml.Linq;

#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

//EC-2573: Regression Tests for Exception in SPL

namespace EnterpriseDB.EDBClient.Tests.SPL
{
    [NonParallelizable]
    public class EDBControlStructuresUserDefExptTest : EPASTestBase
    {
        EDBConnection? conn = null;

        //This is not normal setup, we call it explicitly.
        public void Init(string table, string func, string procPurchase, string procRecord, string procRaise, string pkgAr)
        {
            conn = OpenConnection();

            Execute("DROP PROCEDURE " + procRecord);
            Execute("DROP PROCEDURE " + procRaise);
            Execute("DROP FUNCTION " + func);
            Execute("DROP PACKAGE BODY " + pkgAr);
            Execute("DROP PACKAGE " + pkgAr);
            Execute("DROP PROCEDURE " + procPurchase);

            Execute("DROP TABLE " + table + " CASCADE");

            Execute("CREATE TABLE " + table + "(cust_id NUMBER(8), cust_name VARCHAR2(20), balance NUMBER(10,2), "
                  + "stmt_amount NUMBER(10,2))");
            var add1001 = "insert into " + table + " (cust_id, cust_name, balance, stmt_amount) "
                    + " values (1001, 'Mike', 20000,1000);";
            Execute(add1001);
            var add1002 = "insert into " + table + " (cust_id, cust_name, balance, stmt_amount) "
                    + " values (1002, 'Mike', 20000,1000);";
            Execute(add1002);
            // This example declares a user-defined exception in a package
            var createPkg = "CREATE OR REPLACE PACKAGE " + pkgAr + " AS\n"
                             + "  overdrawn EXCEPTION;\n"
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
            // The procedure purchase calls the check_balance procedure.
            // If p_amount is greater than p_balance, check_balance raises an exception.
            var purPro = "CREATE OR REPLACE PROCEDURE " + procPurchase + "(customerID INT, amount NUMERIC)\n"
                          + "AS\n"
                          + "  BEGIN\n"
                          + "     " + pkgAr + ".check_balance(" + func + "(customerid), amount);\n"
                          + "       " + procRecord + "(customerid, amount);\n"
                          + "  EXCEPTION\n"
                          + "     WHEN " + pkgAr + ".overdrawn THEN\n"
                          + "       " + procRaise + "(customerid, amount*1.5);\n"
                          + "  END;";
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
            var raiseLimitPro = "CREATE OR REPLACE procedure " + procRaise + "( \n"
                    + "   customerID INT, amount NUMERIC)\n"
                    + "AS\n"
                    + "BEGIN\n"
                    + "      update " + table + " set balance = amount \n"
                    + "   WHERE cust_id = customerId; \n"
                    + "EXCEPTION\n"
                    + "   WHEN NO_DATA_FOUND THEN\n"
                    + "      DBMS_OUTPUT.PUT_LINE('Customer # ' || customerId \n"
                    + "       || ' not found');\n"
                    + "END;";
            Execute(raiseLimitPro);
            var recordPurPro = "CREATE OR REPLACE procedure " + procRecord + " (\n"
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

        private double getCustomerStmtAmount(string table, int custId)
        {
            var command = "select stmt_amount from " + table + " where cust_id=" + custId;

            var selectCommand = new EDBCommand(command, conn);
            var selectResult = selectCommand.ExecuteReader();
            selectResult.Read();
            var amount = selectResult.GetDouble(0);
            selectResult.Close();

            return amount;
        }

        private double getCustomerBalance(string table, int custId)
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
            Init("customer3", "getcustomerbalance3", "purchase3", "record_purchase3", "raise_credit_limit3", "ar3");
            // call the purchase function with amount less than balance
            var commandText = "purchase3(:param1,:param2)";

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

            Assert.AreEqual(4000, getCustomerStmtAmount("customer3", 1001), 0.00);
            Assert.AreEqual(17000, getCustomerBalance("customer3", 1001), 0.00);
        }

        [Test]
        public void PurchaseWithExceptionTest()
        {
            Init("customer4", "getcustomerbalance4", "purchase4", "record_purchase4", "raise_credit_limit4", "ar4");
            // call the purchase function with amount more than balance
            // will raises overdrawn exception
            var commandText = "purchase4(:param1,:param2)";

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

            Assert.AreEqual(1000, getCustomerStmtAmount("customer4", 1002), 0.00);
            Assert.AreEqual(45000, getCustomerBalance("customer4", 1002), 0.00);
        }
    }
}
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8604 // Possible null reference argument.
