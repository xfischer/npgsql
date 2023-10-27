using System;
using NUnit.Framework;
using EnterpriseDB.EDBClient;
using System.Data;
using System.Data.SqlTypes;
using System.Xml.Linq;
using System.Threading;
using System.Collections.Generic;
using System.Collections;

#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

//EC-2641: Regression Tests for Object types and objects in SPL

namespace EnterpriseDB.EDBClient.Tests.SPL
{
    internal class EDBObjectTest : TestBase
    {
        EDBConnection? conn = null;

        private static string[] EMP_DISPLAY_RESULT = {
            "Employee No   : 9001", "Name          : JONES",
            "Street        : 123 MAIN STREET",
            "City/State/Zip: EDISON, NJ 08817"
            };
        private static string[] DEPT_DISPLAY_RESULT = {
            "Dept No    : 20",
            "Dept Name  : RESEARCH"
            };
        private static string[] CONSTUCTOR_METHOD_RESULT = {
            "Boston",
            "MA"
            };
        private static string[] GET_DNAME_RESULT = {
            "RESEARCH"
            };

        [SetUp]
        public void Init()
        {
            conn = OpenConnection();
            TestUtil.EnsureEDBAdvancedServer(conn);

            Execute("DROP TYPE BODY address");
            Execute("DROP TYPE address");
            Execute("DROP TYPE BODY dept_obj_type");
            Execute("DROP TYPE dept_obj_type");
            Execute("DROP TYPE BODY emp_obj_typ");
            Execute("DROP TYPE emp_obj_typ");
            Execute("DROP TYPE addr_obj_typ");
            Execute("DROP FUNCTION postal_code_to_city");
            Execute("DROP FUNCTION postal_code_to_state");

            Execute("DROP TABLE citydata CASCADE");

            // The first example creates the addr_object_type object type that
            // contains only attributes and no methods.
            var addrObjType = "CREATE OR REPLACE TYPE addr_obj_typ AS OBJECT\n"
                               + "(\n"
                               + "    street          VARCHAR2(30),\n"
                               + "    city            VARCHAR2(20),\n"
                               + "    state           CHAR(2),\n"
                               + "    zip             NUMBER(5)\n"
                               + ");";
            Execute(addrObjType);

            // This object type specification creates the emp_obj_typ object type.
            var empObjType = "CREATE OR REPLACE TYPE emp_obj_typ AS OBJECT\n"
                              + "(\n"
                              + "    empno           NUMBER(4),\n"
                              + "    ename           VARCHAR2(20),\n"
                              + "    addr            ADDR_OBJ_TYP,\n"
                              + "    MEMBER PROCEDURE display_emp01(SELF IN OUT emp_obj_typ),\n"
                              + "    MEMBER PROCEDURE display_emp02(SELF IN OUT emp_obj_typ)\n"
                              + ");";
            Execute(empObjType);

            // Object type emp_obj_typ contains member methods named display_emp01 and
            // display_emp02.
            // display_emp01 uses a SELF parameter, which passes the object instance
            // on which the method is invoked.
            // You can also use the SELF parameter in an object type body.
            // display_emp02 using the SELF parameter in the CREATE TYPE BODY command.
            var empObjTypeBody = "CREATE OR REPLACE TYPE BODY emp_obj_typ AS\n"
                    + "    MEMBER PROCEDURE display_emp01 (SELF IN OUT emp_obj_typ)\n"
                    + "    IS\n" + "    BEGIN\n"
                    + "        DBMS_OUTPUT.PUT_LINE('Employee No   : ' || empno);\n"
                    + "        DBMS_OUTPUT.PUT_LINE('Name          : ' || ename);\n"
                    + "        DBMS_OUTPUT.PUT_LINE('Street        : ' || addr.street);\n"
                    + "        DBMS_OUTPUT.PUT_LINE('City/State/Zip: ' || addr.city || ', ' ||\n"
                    + "            addr.state || ' ' || LPAD(addr.zip,5,'0'));\n"
                    + "    END;\n"
                    + "    MEMBER PROCEDURE display_emp02 (SELF IN OUT emp_obj_typ)\n"
                    + "    IS\n"
                    + "    BEGIN\n"
                    + "        DBMS_OUTPUT.PUT_LINE('Employee No   : ' || SELF.empno);\n"
                    + "        DBMS_OUTPUT.PUT_LINE('Name          : ' || SELF.ename);\n"
                    + "        DBMS_OUTPUT.PUT_LINE('Street        : ' || SELF.addr.street);\n"
                    + "        DBMS_OUTPUT.PUT_LINE('City/State/Zip: ' || SELF.addr.city || ', ' ||\n"
                    + "            SELF.addr.state || ' ' || LPAD(SELF.addr.zip,5,'0'));\n"
                    + "    END;\n"
                    + "END;";
            Execute(empObjTypeBody);

            // The following object type specification includes a static function get_dname
            // and a member procedure display_dept
            var deptObjType = "CREATE OR REPLACE TYPE dept_obj_type AS OBJECT (\n"
                    + "    deptno          NUMBER(2),\n"
                    + "    STATIC FUNCTION get_dname(p_deptno IN NUMBER) RETURN VARCHAR2,\n"
                    + "    MEMBER PROCEDURE display_dept\n"
                    + ");";
            Execute(deptObjType);

            // The object type body for dept_obj_type defines a static function named
            // get_dname and a member procedure named display_dept:
            var deptObjTypeBody = "CREATE OR REPLACE TYPE BODY dept_obj_type AS\n"
                    + "    STATIC FUNCTION get_dname(p_deptno IN NUMBER) RETURN VARCHAR2\n"
                    + "    IS\n"
                    + "        v_dname     VARCHAR2(14);\n"
                    + "    BEGIN\n"
                    + "        CASE p_deptno\n"
                    + "            WHEN 10 THEN v_dname := 'ACCOUNTING';\n"
                    + "            WHEN 20 THEN v_dname := 'RESEARCH';\n"
                    + "            WHEN 30 THEN v_dname := 'SALES';\n"
                    + "            WHEN 40 THEN v_dname := 'OPERATIONS';\n"
                    + "            ELSE v_dname := 'UNKNOWN';\n"
                    + "        END CASE;\n"
                    + "        RETURN v_dname;\n"
                    + "    END;\n"
                    + "\n"
                    + "    MEMBER PROCEDURE display_dept\n"
                    + "    IS\n"
                    + "    BEGIN\n"
                    + "        DBMS_OUTPUT.PUT_LINE('Dept No    : ' || SELF.deptno);\n"
                    + "        DBMS_OUTPUT.PUT_LINE('Dept Name  : ' ||\n"
                    + "            dept_obj_type.get_dname(SELF.deptno));\n"
                    + "    END;\n"
                    + "END;";
            Execute(deptObjTypeBody);

            Execute("CREATE TABLE citydata(post_code  VARCHAR2(10),  city VARCHAR2(40), state VARCHAR2(2))");
            Execute("insert into citydata values ('01801','Woburn','MA')");
            Execute("insert into citydata values ('02203','Boston','MA')");

            // Methods used in constructor function in object type address
            var postalCodeToCityFun = "CREATE OR REPLACE FUNCTION postal_code_to_city(p_post_code VARCHAR2)\n"
                    + "    RETURN VARCHAR2\n"
                    + "IS\n"
                    + " v_city         citydata.city%TYPE;"
                    + "BEGIN\n"
                    + "   SELECT city INTO v_city FROM citydata WHERE post_code = p_post_code;"
                    + " RETURN v_city;"
                    + "END;";
            Execute(postalCodeToCityFun);

            // Methods used in constructor function in object type address
            var postalCodeToStateFun = "CREATE OR REPLACE FUNCTION postal_code_to_state(p_post_code VARCHAR2)\n"
                    + "    RETURN VARCHAR2\n"
                    + "IS\n"
                    + " v_state         citydata.state%TYPE;"
                    + "BEGIN\n"
                    + "   SELECT state INTO v_state from citydata WHERE post_code = p_post_code;"
                    + " RETURN v_state;"
                    + "END;";
            Execute(postalCodeToStateFun);

            // To create a custom constructor, declare the constructor function (using the
            // keyword constructor) in
            // the CREATE TYPE command and define the construction function in the CREATE
            // TYPE BODY command.
            var addressType = "CREATE OR REPLACE TYPE address AS OBJECT\n"
                    + "(\n"
                    + "  street_address VARCHAR2(40),\n"
                    + "  postal_code    VARCHAR2(10),\n"
                    + "  city           VARCHAR2(40),\n"
                    + "  state          VARCHAR2(2),\n"
                    + "\n"
                    + "  CONSTRUCTOR FUNCTION address\n"
                    + "   (\n"
                    + "     street_address VARCHAR2,\n"
                    + "     postal_code VARCHAR2\n"
                    + "    ) RETURN self AS RESULT\n"
                    + ")";
            Execute(addressType);

            var addressTypeBody = "CREATE OR REPLACE TYPE BODY address AS\n"
                    + " CONSTRUCTOR FUNCTION address\n"
                    + "  (\n"
                    + "    street_address VARCHAR2,\n"
                    + "    postal_code VARCHAR2\n"
                    + "   ) RETURN self AS RESULT\n"
                    + " IS\n" + "   BEGIN\n"
                    + "     self.street_address := street_address;\n"
                    + "     self.postal_code := postal_code;\n"
                    + "     self.city := postal_code_to_city(postal_code);\n"
                    + "     self.state := postal_code_to_state(postal_code);\n"
                    + "     RETURN;\n"
                    + "   END;\n"
                    + "END;";
            Execute(addressTypeBody);

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

        private void DoMemberMethodTest(string procedure)
        {
            conn.ReloadTypes();

            //Close and reopen the connection so that custom types are reloaded.
            TestUtil.closeDB(conn);
            EDBConnection.GlobalTypeMapper.MapComposite<addr_obj_typ>("public.addr_obj_typ");
            EDBConnection.GlobalTypeMapper.MapComposite<emp_obj_typ>("public.emp_obj_typ");

            conn = OpenConnection();

            // Call object member method
            var commandText = procedure;

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
                using (var cstmt = new EDBCommand(commandText, conn))
                {
                    cstmt.CommandType = CommandType.StoredProcedure;
                    EDBCommandBuilder.DeriveParameters(cstmt);

                    var address = new addr_obj_typ()
                    {
                        street = "123 MAIN STREET",
                        city = "EDISON",
                        state = "NJ",
                        zip = 8817
                    };

                    var emp = new emp_obj_typ()
                    {
                        empno = 9001,
                        ename = "JONES",
                        addr = address
                    };
                    cstmt.Parameters[0].Value = emp;
                    cstmt.Prepare();
                    cstmt.ExecuteNonQuery();
                }
                mre.WaitOne(5000);
                Assert.AreEqual(EMP_DISPLAY_RESULT.Length, notices.Count);
                for (var i = 0; i < notices.Count; i++)
                {
                    var notice = (PostgresNotice?)notices[i];
                    Assert.AreEqual(EMP_DISPLAY_RESULT[i], notice.MessageText);
                }
            }
            finally
            {
                conn.Notice -= action;
            }
            mre.Close();
        }

        [Test, Ignore("Composite type functionality has changed. Need to look into")]
        public void MemberMethodTest()
        {
            DoMemberMethodTest("emp_obj_typ.display_emp01");
        }

        [Test, Ignore("Composite type functionality has changed. Need to look into")]
        public void MemberMememberMethodUsingSelfParameterTestthodTest()
        {
            DoMemberMethodTest("emp_obj_typ.display_emp02");
        }

        [Test]
        //[Ignore("EC-2640: 42601: missing \";\" at end of SQL statement")]
        public void StaticMethodTest()
        {
            conn.ReloadTypes();

            //Close and reopen the connection so that custom types are reloaded.
            TestUtil.closeDB(conn);
            EDBConnection.GlobalTypeMapper.MapComposite<dept_obj_type>("public.dept_obj_type");

            conn = OpenConnection();

            // member method display_dept used static method get_dname
            Execute("DROP PROCEDURE StaticMethod_SP;");

            var sql = "CREATE OR REPLACE PROCEDURE StaticMethod_SP()\n"
                      + " IS\n"
                      + " DECLARE\n"
                                + "    v_dept           DEPT_OBJ_TYPE;\n"
                    + "BEGIN\n"
                    + "    v_dept := dept_obj_type (20);\n"
                    + "     v_dept.display_dept();"
                    + "END;";

            Execute(sql);

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
                using (var cstmt = new EDBCommand("StaticMethod_SP", conn))
                {
                    cstmt.CommandType = CommandType.StoredProcedure;

                    cstmt.Prepare();
                    cstmt.ExecuteNonQuery();
                }
                mre.WaitOne(5000);
                Assert.AreEqual(DEPT_DISPLAY_RESULT.Length, notices.Count);
                for (var i = 0; i < notices.Count; i++)
                {
                    var notice = (PostgresNotice?)notices[i];
                    Assert.AreEqual(DEPT_DISPLAY_RESULT[i], notice.MessageText);
                }
            }
            finally
            {
                conn.Notice -= action;
            }
            mre.Close();
        }

        [Test]
        //[Ignore("EC-2640: 42601: missing \";\" at end of SQL statement")]
        public void ConstuctorMethodTest()
        {
            // call constructor method
            Execute("DROP PROCEDURE ConstuctorMethod_SP;");

            var sql = "CREATE OR REPLACE PROCEDURE ConstuctorMethod_SP()\n"
                + " IS\n"
                + " DECLARE\n"
                + "  cust_addr address := address('100 Main Street', '02203');\n"
                + "BEGIN\n"
                + "  DBMS_OUTPUT.PUT_LINE(cust_addr.city);\n"
                + "  DBMS_OUTPUT.PUT_LINE(cust_addr.state);\n"
                + "END;";

            Execute(sql);

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
                using (var cstmt = new EDBCommand("ConstuctorMethod_SP", conn))
                {
                    cstmt.CommandType = CommandType.StoredProcedure;

                    cstmt.Prepare();
                    cstmt.ExecuteNonQuery();
                }
                mre.WaitOne(5000);
                Assert.AreEqual(CONSTUCTOR_METHOD_RESULT.Length, notices.Count);
                for (var i = 0; i < notices.Count; i++)
                {
                    var notice = (PostgresNotice?)notices[i];
                    Assert.AreEqual(CONSTUCTOR_METHOD_RESULT[i], notice.MessageText);
                }
            }
            finally
            {
                conn.Notice -= action;
            }
            mre.Close();
        }

        [Test]
        //[Ignore("EC-2640: 42601: missing \";\" at end of SQL statement")]
        public void ReferencingAnObjectTest()
        {
            // This example displays the values assigned to the emp_obj_typ object.
            Execute("DROP PROCEDURE ReferencingAnObject_SP;");

            var sql = "CREATE OR REPLACE PROCEDURE ReferencingAnObject_SP()\n"
                + " IS\n"
                + " DECLARE\n"
                + "    v_emp          EMP_OBJ_TYP;\n" + "BEGIN\n"
                + "    v_emp := emp_obj_typ(9001,'JONES',\n"
                + "        addr_obj_typ('123 MAIN STREET','EDISON','NJ',08817));\n"
                + "    DBMS_OUTPUT.PUT_LINE('Employee No   : ' || v_emp.empno);\n"
                + "    DBMS_OUTPUT.PUT_LINE('Name          : ' || v_emp.ename);\n"
                + "    DBMS_OUTPUT.PUT_LINE('Street        : ' || v_emp.addr.street);\n"
                + "    DBMS_OUTPUT.PUT_LINE('City/State/Zip: ' || v_emp.addr.city || ', ' ||\n"
                + "        v_emp.addr.state || ' ' || LPAD(v_emp.addr.zip,5,'0'));\n"
                + "END;";

            Execute(sql);

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
                using (var cstmt = new EDBCommand("ReferencingAnObject_SP", conn))
                {
                    cstmt.CommandType = CommandType.StoredProcedure;

                    cstmt.Prepare();
                    cstmt.ExecuteNonQuery();
                }
                mre.WaitOne(5000);
                Assert.AreEqual(EMP_DISPLAY_RESULT.Length, notices.Count);
                for (var i = 0; i < notices.Count; i++)
                {
                    var notice = (PostgresNotice?)notices[i];
                    Assert.AreEqual(EMP_DISPLAY_RESULT[i], notice.MessageText);
                }
            }
            finally
            {
                conn.Notice -= action;
            }
            mre.Close();
        }

        [Test]
        //[Ignore("EC-2640: 42601: missing \";\" at end of SQL statement")]
        public void ReferencingAnObjectWithMemberMethodTest()
        {
            // You can duplicate the results of the previous anonymous block by calling
            // the member procedure display_emp.
            Execute("DROP PROCEDURE ReferencingAnObjectWithMemberMethod_SP;");

            var sql = "CREATE OR REPLACE PROCEDURE ReferencingAnObjectWithMemberMethod_SP()\n"
                + " IS\n"
                + " DECLARE\n"
                + "    v_emp          EMP_OBJ_TYP;\n"
                + "BEGIN\n"
                + "    v_emp := emp_obj_typ(9001,'JONES',\n"
                + "        addr_obj_typ('123 MAIN STREET','EDISON','NJ',08817));\n"
                + "    v_emp.display_emp01;\n"
                + "END;";

            Execute(sql);

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
                using (var cstmt = new EDBCommand("ReferencingAnObjectWithMemberMethod_SP", conn))
                {
                    cstmt.CommandType = CommandType.StoredProcedure;

                    cstmt.Prepare();
                    cstmt.ExecuteNonQuery();
                }
                mre.WaitOne(5000);
                Assert.AreEqual(EMP_DISPLAY_RESULT.Length, notices.Count);
                for (var i = 0; i < notices.Count; i++)
                {
                    var notice = (PostgresNotice?)notices[i];
                    Assert.AreEqual(EMP_DISPLAY_RESULT[i], notice.MessageText);
                }
            }
            finally
            {
                conn.Notice -= action;
            }
            mre.Close();
        }

        [Test]
        //[Ignore("EC-2640: 42601: missing \";\" at end of SQL statement")]
        public void ReferencingAnObjectDisplayDeptTest()
        {
            //This anonymous block creates an instance of dept_obj_type and calls
            //the member procedure display_dept
            Execute("DROP PROCEDURE ReferencingAnObjectDisplayDept_SP;");

            var sql = "CREATE OR REPLACE PROCEDURE ReferencingAnObjectDisplayDept_SP()\n"
                + " IS\n"
                + " DECLARE\n"
                + "    v_dept          DEPT_OBJ_TYPE := dept_obj_type (20);\n"
                + "BEGIN\n"
                + "    v_dept.display_dept;\n"
                + "END;";

            Execute(sql);

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
                using (var cstmt = new EDBCommand("ReferencingAnObjectDisplayDept_SP", conn))
                {
                    cstmt.CommandType = CommandType.StoredProcedure;

                    cstmt.Prepare();
                    cstmt.ExecuteNonQuery();
                }
                mre.WaitOne(5000);
                Assert.AreEqual(DEPT_DISPLAY_RESULT.Length, notices.Count);
                for (var i = 0; i < notices.Count; i++)
                {
                    var notice = (PostgresNotice?)notices[i];
                    Assert.AreEqual(DEPT_DISPLAY_RESULT[i], notice.MessageText);
                }
            }
            finally
            {
                conn.Notice -= action;
            }
            mre.Close();
        }

        [Test]
        //[Ignore("EC-2640: 42601: missing \";\" at end of SQL statement")]
        public void ReferencingAnObjectGetDnameTest()
        {
            //You can call the static function defined in dept_obj_type
            //directly by qualifying it by the object type name as follows:
            Execute("DROP PROCEDURE ReferencingAnObjectGetDname_SP;");

            var sql = "CREATE OR REPLACE PROCEDURE ReferencingAnObjectGetDname_SP()\n"
                + " IS\n"
                + "BEGIN\n"
                + "    DBMS_OUTPUT.PUT_LINE(dept_obj_type.get_dname(20));\n"
                + "END;";

            Execute(sql);

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
                using (var cstmt = new EDBCommand("ReferencingAnObjectGetDname_SP", conn))
                {
                    cstmt.CommandType = CommandType.StoredProcedure;

                    cstmt.Prepare();
                    cstmt.ExecuteNonQuery();
                }
                mre.WaitOne(5000);
                Assert.AreEqual(GET_DNAME_RESULT.Length, notices.Count);
                for (var i = 0; i < notices.Count; i++)
                {
                    var notice = (PostgresNotice?)notices[i];
                    Assert.AreEqual(GET_DNAME_RESULT[i], notice.MessageText);
                }
            }
            finally
            {
                conn.Notice -= action;
            }
            mre.Close();
        }
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public class addr_obj_typ
    {
        public string street;
        public string city;
        public string state;
        public decimal zip;
    }

    public class emp_obj_typ
    {
        public decimal empno;
        public string ename;
        public addr_obj_typ addr;
    }

    public class dept_obj_type
    {
        public decimal deptno;
    }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8604 // Possible null reference argument.

