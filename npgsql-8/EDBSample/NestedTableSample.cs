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

        public class Employee
        {
            [PgName("empno")]
            public decimal Number;
            [PgName("ename")]
            public string? Name;
        }

        public static async Task Sample_NestedTableTypesAsync(string ConnectionString)
        {
            var dataSourceBuilder = new EDBDataSourceBuilder(ConnectionString);
            dataSourceBuilder.MapComposite<Employee>("pkgextendtest.emp_rec_typ");
            await using var dataSource = dataSourceBuilder.Build();
            await using var connection = await dataSource.OpenConnectionAsync();

            try
            {
                await CreatePackageAsync(connection);

                var commandText = "pkgExtendTest.nestedTableExtendTest";
                var cstmt = new EDBCommand(commandText, connection);
                cstmt.CommandType = CommandType.StoredProcedure;

                var tableOfParam = cstmt.Parameters.Add(new EDBParameter()
                {
                    Direction = ParameterDirection.Output,
                    DataTypeName = "pkgextendtest.emp_tbl_typ"
                });
                
                await cstmt.PrepareAsync();
                await cstmt.ExecuteNonQueryAsync();

                List<object>? employees = tableOfParam.Value as List<object>;
                if (employees == null)
                {
                    Console.WriteLine($"No employee found");
                    return;
                }

                foreach(var employeeRecord in employees)
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
                await CleanupAsync(connection);
            }
        }

        private async static Task CreatePackageAsync(EDBConnection connection)
        {
            var createPackage = """
                CREATE OR REPLACE PACKAGE pkgExtendTest IS 
                   TYPE emp_rec_typ IS RECORD ( 
                      empno  NUMBER(4), 
                      ename       VARCHAR2(10) 
                     );
                   TYPE emp_tbl_typ IS TABLE OF emp_rec_typ; 
                   PROCEDURE nestedTableExtendTest(emp_tbl OUT emp_tbl_typ);
                END pkgExtendTest;
                """;
            using (var com = new EDBCommand(createPackage, connection) { CommandType = CommandType.Text })
            {
                await com.ExecuteNonQueryAsync();
            }

            var createPackageBody = """
                    CREATE OR REPLACE PACKAGE BODY pkgExtendTest IS
                	PROCEDURE nestedTableExtendTest(emp_tbl OUT emp_tbl_typ) IS
                   	DECLARE 
                      CURSOR emp_cur IS SELECT empno, ename FROM emp WHERE ROWNUM <= 10 order by empno; 
                      i  INTEGER := 0; 
                	BEGIN
                	    emp_tbl := emp_tbl_typ(); 
                	    FOR r_emp IN emp_cur LOOP 
                	        i := i + 1; 
                	        emp_tbl.EXTEND; 
                	        emp_tbl(i) := r_emp; 
                	    END LOOP; 
                 	END nestedTableExtendTest;
                END pkgExtendTest;
                """;
            using (var com = new EDBCommand(createPackageBody, connection) { CommandType = CommandType.Text })
            {
                await com.ExecuteNonQueryAsync();
            }

            await connection.ReloadTypesAsync();
        }

        private async static Task CleanupAsync(EDBConnection connection)
        {
            var dropPackageBody = "DROP PACKAGE BODY pkgExtendTest";
            var dropPackage = "DROP PACKAGE pkgExtendTest";

            using (var com = new EDBCommand(dropPackageBody, connection) { CommandType = CommandType.Text })
            {
                await com.ExecuteNonQueryAsync();
            }
            using (var com = new EDBCommand(dropPackage, connection) { CommandType = CommandType.Text })
            {
                await com.ExecuteNonQueryAsync();
            }
        }
    }
}
