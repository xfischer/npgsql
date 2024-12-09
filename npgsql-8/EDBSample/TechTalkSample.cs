using EnterpriseDB.EDBClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDBSample
{
    internal class TechTalkSample : IDisposable
    {
        private readonly EDBDataSource dataSource;
        private readonly EDBConnection conn;
        private bool disposedValue;

        public TechTalkSample(string connectionString)
        {
            var dataSourceBuilder = new EDBDataSourceBuilder(connectionString);
            dataSource = dataSourceBuilder.Build();
            conn = dataSource.OpenConnection();


            using var command = new EDBCommand("""
                CREATE OR REPLACE FUNCTION totalEmployees()
                RETURNS integer AS $total$
                declare
                	total integer;
                BEGIN
                   SELECT count(*) into total FROM emp;
                   RETURN total;
                END;
                $total$ LANGUAGE plpgsql;
                """
                , conn);
            command.ExecuteNonQuery();

            command.CommandText = """
                CREATE OR REPLACE FUNCTION totalEmployeesByManager(p_mgr numeric)
                RETURNS integer AS $total$
                declare
                	total integer;
                BEGIN
                   SELECT count(*) into total FROM emp WHERE mgr = p_mgr;
                   RETURN total;
                END;
                $total$ LANGUAGE plpgsql;
                """;
            command.ExecuteNonQuery();

            command.CommandText = """
                CREATE OR REPLACE FUNCTION statsEmployeesByManager(
                                                                    IN p_mgr numeric,
                                                                    OUT p_count integer,
                                                                    OUT p_avgSalary numeric
                                                                    )
                AS $$
                    BEGIN
                       SELECT count(*), avg(sal)
                        INTO p_count, p_avgSalary
                       FROM emp
                       WHERE mgr = p_mgr;
                    END;
                $$ LANGUAGE plpgsql;
                """;
            command.ExecuteNonQuery();

            command.CommandText = """
                CREATE OR REPLACE FUNCTION mixArgFunc_test(a INOUT NUMERIC, b OUT NUMERIC, c IN NUMERIC)
                    RETURN int
                AS
                BEGIN
                    b:=c;
                    a:=a+a;
                    return b-1;
                END;
                """;
            command.ExecuteNonQuery();
        }


        internal async Task RunAsync()
        {
            //await RunQueryAsync();
            Console.WriteLine("Connection is open. Next query : SELECT * FROM emp WHERE deptno = @dept");
            Console.ReadLine();

            await RunQueryWithParamAsync();

            Console.WriteLine("Next query : SPL function call");
            Console.WriteLine("""
                CREATE OR REPLACE FUNCTION mixArgFunc_test(a INOUT NUMERIC, b OUT NUMERIC, c IN NUMERIC)
                    RETURN int
                AS
                BEGIN
                    b:=c;
                    a:=a+a;
                    return b-1;
                END;
                """);
            Console.ReadLine();

            await RunSPLFunctionAsync();

            Console.WriteLine("Done");
            Console.ReadLine();

            //await RunQueryPreparedWithParamAsync();

            //await RunScalarFunctionCallAsync();

            //await RunScalarFunctionParameterCallAsync();

            //await RunFunctionOutParameterCallAsync();
        }

        private async Task RunFunctionOutParameterCallAsync()
        {
            // >P/B/D/E/S
            // <1/2/T/D/C/Z

            using var command = new EDBCommand("SELECT statsEmployeesByManager(@p_mgr)", conn);
            command.Parameters.AddWithValue("@p_mgr", 7698);
            //command.Parameters.Add(new EDBParameter("@p_count", null) { Direction = System.Data.ParameterDirection.Output });
            //command.Parameters.Add(new EDBParameter("@p_avgSalary", null) { Direction = System.Data.ParameterDirection.Output });
            var outTuple = await command.ExecuteScalarAsync() as object[];
            if (outTuple != null)
            {
                Console.WriteLine($"statsEmployeesByManager(7698): count: {outTuple[0]}, avg salary: {outTuple[1]}");
            }
        }

        private async Task RunScalarFunctionParameterCallAsync()
        {
            using var command = new EDBCommand("SELECT totalEmployeesByManager(@managerId)", conn);
            command.Parameters.AddWithValue("@managerId", 7698);
            var count = await command.ExecuteScalarAsync();
            Console.WriteLine($"totalEmployeesByManager(7698): {count}");
        }

        private async Task RunScalarFunctionCallAsync()
        {
            using var command = new EDBCommand("SELECT totalEmployees()", conn);
            var count = await command.ExecuteScalarAsync();
            Console.WriteLine($"totalEmployees(): {count}");
        }

        internal async Task RunQueryAsync()
        {
            // >P/B/D/E/S
            // <1/2/T/D/D/D/D/D/C/Z
            using var command = new EDBCommand("SELECT * FROM emp WHERE deptno = 20", conn);
            using var reader = await command.ExecuteReaderAsync();


            // Display results
            var strings = new List<string>();
            var firstRow = true;
            while (await reader.ReadAsync())
            {
                if (firstRow)
                {
                    for (var i = 0; i < reader.FieldCount; i++)
                    {
                        strings.Add(reader.GetName(i));
                    }
                    Console.WriteLine(string.Join(", ", strings));
                    strings.Clear();
                    firstRow = false;
                }

                for (var i = 0; i < reader.FieldCount; i++)
                {
                    strings.Add(reader[i].ToString()!);
                }
                Console.WriteLine(string.Join(" ", strings));
                strings.Clear();
            }
        }

        internal async Task RunQueryWithParamAsync()
        {
            // >P/B/D/E/S
            // <1/2/T/D/D/D/D/D/C/Z
            using var command = new EDBCommand("SELECT * FROM emp WHERE deptno = @dept", conn);
            command.Parameters.AddWithValue("@dept", 20);
            using var reader = await command.ExecuteReaderAsync();


            // Display results
            var strings = new List<string>();
            var firstRow = true;
            while (await reader.ReadAsync())
            {
                if (firstRow)
                {
                    for (var i = 0; i < reader.FieldCount; i++)
                    {
                        strings.Add(reader.GetName(i));
                    }
                    Console.WriteLine(string.Join(", ", strings));
                    strings.Clear();
                    firstRow = false;
                }

                for (var i = 0; i < reader.FieldCount; i++)
                {
                    strings.Add(reader[i].ToString()!);
                }
                Console.WriteLine(string.Join(" ", strings));
                strings.Clear();
            }
        }

        internal async Task RunSPLFunctionAsync()
        {
            // >P/B/D/E/S
            // <1/2/T/D/D/D/D/D/C/Z
            using var command = new EDBCommand("mixArgFunc_test(:paramInOut, :paramOut, :paramIn)", conn);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(new EDBParameter("paramInOut", 10m) { EDBDbType = EDBTypes.EDBDbType.Numeric, Size = 10, Direction = ParameterDirection.InputOutput });
            command.Parameters.Add(new EDBParameter("paramOut", 10m) { EDBDbType = EDBTypes.EDBDbType.Numeric, Size = 10, Direction = ParameterDirection.Output });
            command.Parameters.Add(new EDBParameter("paramIn", 10m) { EDBDbType = EDBTypes.EDBDbType.Numeric, Size = 10, Direction = ParameterDirection.Input });
            command.Parameters.Add(new EDBParameter("paramRetVal", 4) { Direction = ParameterDirection.ReturnValue });
            //command.Parameters.Add(new EDBParameter("paramInOut", EDBTypes.EDBDbType.Numeric, 10, "paramInOut", ParameterDirection.InputOutput, false, 4, 4, System.Data.DataRowVersion.Current, 1));
            //command.Parameters.Add(new EDBParameter("paramOut", EDBTypes.EDBDbType.Numeric, 10, "paramOut", ParameterDirection.Output, false, 4, 4, System.Data.DataRowVersion.Current, 1));
            //command.Parameters.Add(new EDBParameter("paramIn", EDBTypes.EDBDbType.Numeric, 10, "paramIn", ParameterDirection.Input, false, 4, 4, System.Data.DataRowVersion.Current, 1));
            //command.Parameters.Add(new EDBParameter("paramRetVal", EDBTypes.EDBDbType.Integer, 4, "paramRetVal", ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, 1));

            await command.PrepareAsync();

            command.Parameters["paramInOut"].Value = 10;
            command.Parameters["paramIn"].Value = 25;

            using EDBDataReader reader = await command.ExecuteReaderAsync();
        }

        internal async Task RunQueryPreparedWithParamAsync()
        {
            // >P/B/D/E/S
            // <1/2/T/D/D/D/D/D/C/Z
            using var command = new EDBCommand("SELECT * FROM emp WHERE deptno = @dept", conn);
            command.Parameters.AddWithValue("@dept", 20);

            await command.PrepareAsync();
            using var reader = await command.ExecuteReaderAsync();


            // Display results
            var strings = new List<string>();
            var firstRow = true;
            while (await reader.ReadAsync())
            {
                if (firstRow)
                {
                    for (var i = 0; i < reader.FieldCount; i++)
                    {
                        strings.Add(reader.GetName(i));
                    }
                    Console.WriteLine(string.Join(", ", strings));
                    strings.Clear();
                    firstRow = false;
                }

                for (var i = 0; i < reader.FieldCount; i++)
                {
                    strings.Add(reader[i].ToString()!);
                }
                Console.WriteLine(string.Join(" ", strings));
                strings.Clear();
            }
        }

        #region Disposal

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    conn.Dispose();
                    dataSource.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
