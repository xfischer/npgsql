using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Reports;
using EnterpriseDB.EDBClient;
using System.Data;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EDBBenchmark
{
    [MemoryDiagnoser]
    [Config(typeof(EDBManualConfig))]
    public class EPASBenchmark
    {

        [Benchmark]
        //[BenchmarkCategory("EDBPerfIssue")]
        public async Task<string> StoredProcedureParamValuesAsync()
        {
            StringBuilder output = new StringBuilder();
            using (var conn = BenchmarkEnvironment.OpenConnection())
            using (var callable_command = new EDBCommand("emp_query(:p_deptno,:p_empno,:p_ename,:p_job,:p_hiredate,:p_sal)", conn))
            {
                callable_command.CommandType = CommandType.StoredProcedure;
                callable_command.Parameters.Add(new EDBParameter("p_deptno", EDBTypes.EDBDbType.Numeric, 10, "p_deptno", ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 20));
                callable_command.Parameters.Add(new EDBParameter("p_empno", EDBTypes.EDBDbType.Numeric, 10, "p_empno", ParameterDirection.InputOutput, false, 2, 2, System.Data.DataRowVersion.Current, 7369));
                callable_command.Parameters.Add(new EDBParameter("p_ename", EDBTypes.EDBDbType.Varchar, 10, "p_ename", ParameterDirection.InputOutput, false, 2, 2, System.Data.DataRowVersion.Current, "SMITH"));
                callable_command.Parameters.Add(new EDBParameter("p_job", EDBTypes.EDBDbType.Varchar, 10, "p_job", ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));
                callable_command.Parameters.Add(new EDBParameter("p_hiredate", EDBTypes.EDBDbType.Date, 200, "p_hiredate", ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));
                callable_command.Parameters.Add(new EDBParameter("p_sal", EDBTypes.EDBDbType.Numeric, 200, "p_sal", ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));
                await callable_command.PrepareAsync();

                callable_command.Parameters[0].Value = 20;
                callable_command.Parameters[1].Value = 7369;

                using (var reader = await callable_command.ExecuteReaderAsync())
                {
                    var fc = reader.FieldCount;
                    for (var i = 0; i < (fc + 1); i++)
                        output.AppendLine($"RESULT[{i}]={callable_command.Parameters[i].Value.ToString()}");
                    reader.Close();
                    conn.Close();
                }
            }

            return output.ToString();
        }

        //[Benchmark]
        //public async Task<string> StoredProcedureParamPropertiesAsync()
        //{
        //    using var conn = BenchmarkEnvironment.OpenConnection();
        //    using EDBCommand callable_command = new EDBCommand("emp_query(:p_deptno,:p_empno,:p_ename,:p_job,:p_hiredate,:p_sal)", conn);
        //    callable_command.CommandType = CommandType.StoredProcedure;
        //    callable_command.Parameters.Add(new EDBParameter("p_deptno", EDBTypes.EDBDbType.Numeric, 10, "p_deptno", ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 20));
        //    callable_command.Parameters.Add(new EDBParameter("p_empno", EDBTypes.EDBDbType.Numeric, 10, "p_empno", ParameterDirection.InputOutput, false, 2, 2, System.Data.DataRowVersion.Current, 7369));
        //    callable_command.Parameters.Add(new EDBParameter("p_ename", EDBTypes.EDBDbType.Varchar, 10, "p_ename", ParameterDirection.InputOutput, false, 2, 2, System.Data.DataRowVersion.Current, "SMITH"));
        //    callable_command.Parameters.Add(new EDBParameter("p_job", EDBTypes.EDBDbType.Varchar, 10, "p_job", ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));
        //    callable_command.Parameters.Add(new EDBParameter("p_hiredate", EDBTypes.EDBDbType.Date, 200, "p_hiredate", ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));
        //    callable_command.Parameters.Add(new EDBParameter("p_sal", EDBTypes.EDBDbType.Numeric, 200, "p_sal", ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));

        //    await callable_command.PrepareAsync();

        //    callable_command.Parameters[0].Value = 20;
        //    callable_command.Parameters[1].Value = 7369;

        //    using var result = callable_command.ExecuteReader();
        //    StringBuilder output = new StringBuilder();
        //    for (int i = 0; i < callable_command.Parameters.Count; i++)
        //        output.AppendLine($"Parameter[\"{callable_command.Parameters[i].ParameterName}\"]={callable_command.Parameters[i].Value}");

        //    if (await result.ReadAsync())
        //    {
        //        var fc = result.FieldCount;

        //        output.AppendLine("------- Field values ");
        //        for (var i = 0; i < fc; i++)
        //        {
        //            output.AppendLine($"RESULT[{i}]={result[i]}");
        //        }
        //        output.AppendLine("------- Various properties ");
        //        for (var i = 0; i < fc; i++)
        //        {
        //            output.AppendLine($"-- Field {i}");

        //            output.AppendLine($"GetValue[{i}]={result.GetValue(i)}");
        //            //output.AppendLine($"GetEDBValue[{i}]={result.GetEDBValue(i)}");

        //            output.AppendLine($"GetDataTypeName[{i}]={result.GetDataTypeName(i)}");
        //            output.AppendLine($"GetDataTypeOID[{i}]={result.GetDataTypeOID(i)}");
        //            output.AppendLine($"GetFieldType[{i}]={result.GetFieldType(i)}");
        //            output.AppendLine($"GetName[{i}]={result.GetName(i)}");
        //            var name = result.GetName(i);
        //            output.AppendLine($"GetOrdinal[{name}]={result.GetOrdinal(name)}");
        //            output.AppendLine($"GetPostgresType[{i}]={result.GetPostgresType(i)}");
        //            output.AppendLine($"IsDBNull[{i}]={await result.IsDBNullAsync(i)}");
        //        }
        //    }
        //    result.Close();
        //    conn.Close();
        //    return output.ToString();
        //}
    }
}
