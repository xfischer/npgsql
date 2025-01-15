using BenchmarkDotNet.Attributes;
using EnterpriseDB.EDBClient;
using Npgsql;
using System.Text;

namespace EDBBenchmark
{

    // needs the following DB setup
    /*
    create table loadtest (id number primary key, largedata1 varchar2 (12), largedata2 varchar2 (4000), largedata3 clob);

    begin
    for i in 1..350000 loop
        insert into loadtest (id, largedata1, largedata2, largedata3) 
        values ( i, lpad(i, 12, 'x'), lpad('abcd', 4000, 'y'), lpad ('pqrs', 8000, 'ab') );
        if (i%100 = 0) THEN
            commit;
        end if;
    end loop;
    end;

    create index xie1loadtest on loadtest (id);
    */

    [MemoryDiagnoser]
    [Config(typeof(EDBManualConfig))]
    public class EDBBenchmark
    {
        const int NumRows = 200;

        [Benchmark(Baseline =true)]
        [BenchmarkCategory("EDBPerfIssue")]
        public void ReadPK()
        {
            using var conn = BenchmarkEnvironment.OpenNpgsqlConnection();
            using var EDBSeletCommand = new NpgsqlCommand("SELECT EMPNO,ENAME,JOB,MGR,HIREDATE FROM EMP", conn);
            EDBSeletCommand.CommandText = $"SELECT id FROM loadtest LIMIT {NumRows}";
            using (var reader = EDBSeletCommand.ExecuteReader())
            {
                object[] values = new object[reader.FieldCount];
                while (reader.Read())
                {
                    reader.GetValues(values);
                }
            }
        }

        [Benchmark]
        public void ReadAllColumns()
        {
            using var conn = BenchmarkEnvironment.OpenNpgsqlConnection();
            using var SeletCommand = new NpgsqlCommand($"SELECT * FROM loadtest LIMIT {NumRows}", conn);
            using (var reader = SeletCommand.ExecuteReader())
            {
                object[] values = new object[reader.FieldCount];
                while (reader.Read())
                {
                    reader.GetValues(values);
                }
            }
        }

        [Benchmark]
        [BenchmarkCategory("EDBPerfIssue")]
        public async Task<string> SelectStatementAsync()
        {
            var output = new StringBuilder();
            using var conn = BenchmarkEnvironment.OpenNpgsqlConnection();
            using var SeletCommand = new NpgsqlCommand("SELECT EMPNO,ENAME,JOB,MGR,HIREDATE FROM EMP", conn);
            using var SelectResult = await SeletCommand.ExecuteReaderAsync();
            while (await SelectResult.ReadAsync())
            {
                output.AppendLine("Emp No" + " " + SelectResult.GetInt32(0));
                output.AppendLine("Emp Name" + " " + SelectResult.GetString(1));
                if (SelectResult.IsDBNull(2) == false)
                    output.AppendLine("Job" + " " + SelectResult.GetString(2));
                else
                    output.AppendLine("Job" + " null ");
                if (SelectResult.IsDBNull(3) == false)
                    output.AppendLine("Mgr" + " " + SelectResult.GetInt32(3));
                else
                    output.AppendLine("Mgr" + "null");
                if (SelectResult.IsDBNull(4) == false)
                    output.AppendLine("Hire Date" + " " + SelectResult.GetDateTime(4));
                else
                    output.AppendLine("Hire Date" + " null");
                output.AppendLine("---------------------------------");
            }
            SelectResult.Close();
            conn.Close();
            return output.ToString();
        }

        [Benchmark]
        [BenchmarkCategory("EDBPerfIssue")]
        public void BindVariable()
        {
            using var conn = BenchmarkEnvironment.OpenNpgsqlConnection();
            using var EDBCommand = new NpgsqlCommand($"SELECT * FROM loadtest WHERE id = :b", conn);
            int i = 1_000;
            while (i < NumRows + 1_000)
            {
                EDBCommand.Parameters.Clear();
                var param = new NpgsqlParameter() { Value = i, ParameterName = "b" };
                EDBCommand.Parameters.Add(param);
                var reader = EDBCommand.ExecuteReader();
                while (reader.Read())
                {
                    var dummy = reader[0];
                }
                reader.Close();
                i++;
            }
        }






        [Benchmark()]
        [BenchmarkCategory("EDBPerfIssue")]
        public void EDBReadPK()
        {
            using var conn = BenchmarkEnvironment.OpenConnection();
            using var EDBSeletCommand = new EDBCommand("SELECT EMPNO,ENAME,JOB,MGR,HIREDATE FROM EMP", conn);
            EDBSeletCommand.CommandText = $"SELECT id FROM loadtest LIMIT {NumRows}";
            using (var reader = EDBSeletCommand.ExecuteReader())
            {
                object[] values = new object[reader.FieldCount];
                while (reader.Read())
                {
                    reader.GetValues(values);
                }
            }
        }

        [Benchmark]
        public void EDBReadAllColumns()
        {
            using var conn = BenchmarkEnvironment.OpenConnection();
            using var SeletCommand = new EDBCommand($"SELECT * FROM loadtest LIMIT {NumRows}", conn);
            using (var reader = SeletCommand.ExecuteReader())
            {
                object[] values = new object[reader.FieldCount];
                while (reader.Read())
                {
                    reader.GetValues(values);
                }
            }
        }

        [Benchmark]
        [BenchmarkCategory("EDBPerfIssue")]
        public async Task<string> EDBSelectStatementAsync()
        {
            var output = new StringBuilder();
            using var conn = BenchmarkEnvironment.OpenConnection();
            using var SeletCommand = new EDBCommand("SELECT EMPNO,ENAME,JOB,MGR,HIREDATE FROM EMP", conn);
            using var SelectResult = await SeletCommand.ExecuteReaderAsync();
            while (await SelectResult.ReadAsync())
            {
                output.AppendLine("Emp No" + " " + SelectResult.GetInt32(0));
                output.AppendLine("Emp Name" + " " + SelectResult.GetString(1));
                if (SelectResult.IsDBNull(2) == false)
                    output.AppendLine("Job" + " " + SelectResult.GetString(2));
                else
                    output.AppendLine("Job" + " null ");
                if (SelectResult.IsDBNull(3) == false)
                    output.AppendLine("Mgr" + " " + SelectResult.GetInt32(3));
                else
                    output.AppendLine("Mgr" + "null");
                if (SelectResult.IsDBNull(4) == false)
                    output.AppendLine("Hire Date" + " " + SelectResult.GetDateTime(4));
                else
                    output.AppendLine("Hire Date" + " null");
                output.AppendLine("---------------------------------");
            }
            SelectResult.Close();
            conn.Close();
            return output.ToString();
        }

        [Benchmark]
        [BenchmarkCategory("EDBPerfIssue")]
        public void EDBBindVariable()
        {
            using var conn = BenchmarkEnvironment.OpenConnection();
            using var EDBCommand = new EDBCommand($"SELECT * FROM loadtest WHERE id = :b", conn);
            int i = 1_000;
            while (i < NumRows + 1_000)
            {
                EDBCommand.Parameters.Clear();
                var param = new EDBParameter() { Value = i, ParameterName = "b" };
                EDBCommand.Parameters.Add(param);
                var reader = EDBCommand.ExecuteReader();
                while (reader.Read())
                {
                    var dummy = reader[0];
                }
                reader.Close();
                i++;
            }
        }


    }
}
