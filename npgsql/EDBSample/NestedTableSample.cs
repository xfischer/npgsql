using EDBTypes;
using EnterpriseDB.EDBClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDBSample
{
    internal static class NestedTableSample
    {

#pragma warning disable CS0649 //field is never assigned

        // Composite type, will be mapped to the nested table type
        // This will work if field types are convertible from database types
        public class Employee
        {
            [PgName("empno")]
            public decimal Number = 0;
            [PgName("ename")]
            public string? Name;
        }
#pragma warning restore CS0649

        public static void Sample_NestedTableTypes(string ConnectionString)
        {
            var dataSourceBuilder = new EDBDataSourceBuilder(ConnectionString);
            dataSourceBuilder.MapComposite<Employee>("pkgextendtest.emp_rec_typ");

            using (var dataSource = dataSourceBuilder.Build())
            {
                using (var connection = dataSource.OpenConnection())
                {

                    try
                    {
                        CreatePackage(connection);

                        var commandText = "pkgExtendTest.nestedTableExtendTest";
                        var cstmt = new EDBCommand(commandText, connection);
                        cstmt.CommandType = CommandType.StoredProcedure;

                        var tableOfParam = cstmt.Parameters.Add(new EDBParameter()
                        {
                            Direction = ParameterDirection.Output,
                            DataTypeName = "pkgextendtest.emp_tbl_typ"
                        });

                        cstmt.Prepare();
                        cstmt.ExecuteNonQuery();

                        var employees = tableOfParam.Value as List<object>;
                        if (employees == null)
                        {
                            Console.WriteLine($"No employee found");
                            return;
                        }

                        foreach (var employeeRecord in employees)
                        {
                            var employee = employeeRecord as Employee;
                            if (employee != null)
                            {
                                Console.WriteLine($"Employee {employee.Number}: {employee.Name}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                    }
                    finally
                    {
                        Cleanup(connection);
                    }
                }
            }
        }

        // helper methods to create package and cleaning up  
        static void CreatePackage(EDBConnection connection)
        {
            var createPackage = 
"                CREATE OR REPLACE PACKAGE pkgExtendTest IS  \n" +
"                   TYPE emp_rec_typ IS RECORD (  \n" +
"                      empno  NUMBER(4),  \n" +
"                      ename       VARCHAR2(10)  \n" +
"                     ); \n" +
"                   TYPE emp_tbl_typ IS TABLE OF emp_rec_typ;  \n" +
"                   PROCEDURE nestedTableExtendTest(emp_tbl OUT emp_tbl_typ); \n" +
"                END pkgExtendTest; \n";
            using (var com = new EDBCommand(createPackage, connection) { CommandType = CommandType.Text })
            {
                com.ExecuteNonQuery();
            }

            var createPackageBody = 
"                    CREATE OR REPLACE PACKAGE BODY pkgExtendTest IS \n" +
"                    PROCEDURE nestedTableExtendTest(emp_tbl OUT emp_tbl_typ) IS \n" +
"                      DECLARE  \n" +
"                      CURSOR emp_cur IS SELECT empno, ename FROM emp WHERE ROWNUM <= 10 order by empno;  \n" +
"                      i  INTEGER := 0;  \n" +
"                    BEGIN \n" +
"                       emp_tbl := emp_tbl_typ();  \n" +
"                       FOR r_emp IN emp_cur LOOP  \n" +
"                          i := i + 1;  \n" +
"                          emp_tbl.EXTEND;  \n" +
"                          emp_tbl(i) := r_emp;  \n" +
"                       END LOOP;  \n" +
"                    END nestedTableExtendTest; \n" +
"                    END pkgExtendTest; \n";
            using (var com = new EDBCommand(createPackageBody, connection) { CommandType = CommandType.Text })
            {
                com.ExecuteNonQuery();
            }

            connection.ReloadTypes();
        }

        static void Cleanup(EDBConnection connection)
        {
            var dropPackageBody = "DROP PACKAGE BODY pkgExtendTest";
            var dropPackage = "DROP PACKAGE pkgExtendTest";

            using (var com = new EDBCommand(dropPackageBody, connection) { CommandType = CommandType.Text })
            {
                com.ExecuteNonQuery();
            }
            using (var com = new EDBCommand(dropPackage, connection) { CommandType = CommandType.Text })
            {
                com.ExecuteNonQuery();
            }
        }
    }
}
