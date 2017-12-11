using System;
using System.Data;
using EnterpriseDB.EDBClient;
using System.IO;
using NUnit.Framework;
//using NUnit.Framework.Interfaces;
using System.Threading;
using System.Threading.Tasks;


namespace DOTNET
{
	/// <summary>
	/// Utility class for .NET tests
	/// </summary>
	public class TestUtil
	{
		public static bool IsOnBuildServer => Environment.GetEnvironmentVariable("TEAMCITY_VERSION") != null;

		/// <summary>
		/// Calls Assert.Ignore() unless we're on the build server, in which case calls
		/// Assert.Fail(). We don't to miss any regressions just because something was misconfigured
		/// at the build server and caused a test to be inconclusive.
		/// </summary>
		public static void IgnoreExceptOnBuildServer(string message)
		{
			if (IsOnBuildServer)
				Assert.Fail(message);
			else
				Assert.Ignore(message);
		}

		//Opens a connection with database
		public static String defaultConnectionString = System.Configuration.ConfigurationSettings.AppSettings["connectionString"];

		public static EDBConnection openDB(String conString)
		{
			try
			{
				EDBConnection con = new EDBConnection(conString);
				con.Open();
				return con;
			}
			catch(EDBException e)
			{
				throw new Exception(e.ToString());
			}
		}

		public static EDBConnection openDB()
		{
			return openDB(defaultConnectionString);
		}

		public static EDBConnection openDB(EDBConnectionStringBuilder csb)
		{
			return openDB(csb.ToString());
		}

		public static EDBConnection openDBwithoutPooling()
		{
			try
			{
				EDBConnection con = new EDBConnection(defaultConnectionString);
				con.Open();
				return con;
			}
			catch(EDBException e)
			{
				throw new Exception(e.ToString());
			}
		}
		public static void closeDB(EDBConnection con)
		{
			if (con != null)
				con.Close();
		}


		public static bool IsSequential(CommandBehavior behavior)
			=> (behavior & CommandBehavior.SequentialAccess) != 0;

		public static void MinimumPgVersion(EDBConnection conn, string minVersion, string ignoreText = null)
		{
			var min = new Version(minVersion);
			if (conn.PostgreSqlVersion < min)
			{
				var msg = $"Postgresql backend version {conn.PostgreSqlVersion} is less than the required {min}";
				if (ignoreText != null)
					msg += ": " + ignoreText;
				Assert.Ignore(msg);
			}
		}

		public static string GetUniqueIdentifier(string prefix)
			=> prefix + Interlocked.Increment(ref _counter);

		static int _counter;

		public static void createTempTable(EDBConnection con,String table,String columns)
		{
			string strCommandSql = "create temp table " + table + " (" + columns + ")";
			EDBCommand com = new EDBCommand(strCommandSql, con);
			com.CommandType = CommandType.Text;
			try
			{
				// Drop the table
				dropTable(con, table);

				// Now create the table
				com.ExecuteNonQuery();
			}
			catch (EDBException e)
			{
				throw new Exception(e.ToString());
			}
		}

		public static void ExecuteSqlFile(EDBConnection connection, string sqlFile)
		{
			string sql = "";
			if(!File.Exists(sqlFile))
				Console.WriteLine("File Not found"); 
			using (FileStream strm = File.OpenRead(sqlFile))
			{
				StreamReader reader = new StreamReader(strm);
				sql = reader.ReadToEnd();
			}

			connection.Open();
			EDBCommand cmd = new EDBCommand(sql,connection);
			cmd.ExecuteNonQuery();
            connection.Close();

		}

        public static void ExecuteSql(EDBConnection connection, string sql)
        {
            connection.Open();
            EDBCommand cmd = new EDBCommand(sql, connection);
            cmd.ExecuteNonQuery();
            connection.Close();

        }

		public static void dropTable(EDBConnection con, String table)
		{
			
			try
			{
				String strCommandSql = "DROP TABLE " + table;
/*              if (haveMinimumServerVersion(con, "7.3"))
				{
					sql += " CASCADE ";
				}*/
				EDBCommand com = new EDBCommand(strCommandSql, con);
				com.CommandType = CommandType.Text;
				com.ExecuteNonQuery();
			}
			catch (EDBException e)
			{
				// Since every create table issues a drop table
				// it's easy to get a table doesn't exist error.
				// we want to ignore these, but if we're in a
				// transaction then we've got trouble
/*				if (!con.getAutoCommit())
                throw ex;
				
*/				
			}
		}
		/// <summary>
		/// In PG under 9.1 you can't do SELECT pg_sleep(2) in binary because that function returns void and PG doesn't know
		/// how to transfer that. So cast to text server-side.
		/// </summary>
		/// <param name="seconds"></param>
		/// <returns></returns>
		public static EDBCommand CreateSleepCommand(EDBConnection conn, int seconds)
		{
			return new EDBCommand(string.Format("SELECT pg_sleep({0}){1}", seconds, conn.PostgreSqlVersion < new Version(9, 1, 0) ? "::TEXT" : ""), conn);
		}
	}


	public static class EDBConnectionExtensions
	{
		public static int ExecuteNonQuery(this EDBConnection conn, string sql, EDBTransaction tx = null)
		{
			var cmd = tx == null ? new EDBCommand(sql, conn) : new EDBCommand(sql, conn, tx);
			using (cmd)
				return cmd.ExecuteNonQuery();
		}

		//[CanBeNull]
		public static object ExecuteScalar(this EDBConnection conn, string sql, EDBTransaction tx = null)
		{
			var cmd = tx == null ? new EDBCommand(sql, conn) : new EDBCommand(sql, conn, tx);
			using (cmd)
				return cmd.ExecuteScalar();
		}

		public static async Task<int> ExecuteNonQueryAsync(this EDBConnection conn, string sql, EDBTransaction tx = null)
		{
			var cmd = tx == null ? new EDBCommand(sql, conn) : new EDBCommand(sql, conn, tx);
			using (cmd)
				return await cmd.ExecuteNonQueryAsync();
		}

		//[CanBeNull]
		public static async Task<object> ExecuteScalarAsync(this EDBConnection conn, string sql, EDBTransaction tx = null)
		{
			var cmd = tx == null ? new EDBCommand(sql, conn) : new EDBCommand(sql, conn, tx);
			using (cmd)
				return await cmd.ExecuteScalarAsync();
		}
	}
	/// <summary>
	/// Semantic attribute that points to an issue linked with this test (e.g. this
	/// test reproduces the issue)
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
	public class IssueLink : Attribute
	{
		public string LinkAddress { get; private set; }
		public IssueLink(string linkAddress)
		{
			LinkAddress = linkAddress;
		}
	}

	/// <summary>
	/// Causes the test to be ignored on mono
	/// </summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = false)]
	public class MonoIgnore : Attribute//, ITestAction
	{
		readonly string _ignoreText;

		public MonoIgnore(string ignoreText = null) { _ignoreText = ignoreText; }
		/*
		public void BeforeTest(/*[NotNull] ITest test)
		{
			if (Type.GetType("Mono.Runtime") != null)
			{
				var msg = "Ignored on mono";
				if (_ignoreText != null)
					msg += ": " + _ignoreText;
				Assert.Ignore(msg);
			}
		}*/

		//public void AfterTest(/*[NotNull]*/ ITest test) { }
		public ActionTargets Targets => ActionTargets.Test;
	}

	public enum PrepareOrNot
	{
		Prepared,
		NotPrepared
	}

#if NETCOREAPP1_0
    // When using netcoreapp, we use NUnit's portable library which doesn't include TimeoutAttribute
    // (probably because it can't enforce it). So we define it here to allow us to compile, once there's
    // proper support for netcoreapp this should be removed.
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
    class TimeoutAttribute : Attribute
    {
        public TimeoutAttribute(int timeout) {}
    }
#endif

}