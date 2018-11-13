using System;
using System.Data;
using EnterpriseDB.EDBClient;
using System.IO;


namespace DOTNET
{
	/// <summary>
	/// Utility class for .NET tests
	/// </summary>
	public class TestUtil
	{	
		//Opens a connection with database
		public static EDBConnection openDB()
		{
			try
			{
				EDBConnection con = new EDBConnection(System.Configuration.ConfigurationSettings.AppSettings["connectionString"]);
				con.Open();
				return con;
			}
			catch(EDBException e)
			{
				throw new Exception(e.ToString());
			}
		}
		public static EDBConnection openDBwithoutPooling()
		{
			try
			{
				EDBConnection con = new EDBConnection(System.Configuration.ConfigurationSettings.AppSettings["connectionString"]);
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

		public static void ExecuteSql(EDBConnection connection, string sqlFile)
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
	}
}