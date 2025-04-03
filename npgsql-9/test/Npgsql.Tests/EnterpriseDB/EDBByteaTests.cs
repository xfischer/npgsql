using System;
using NUnit.Framework;
using System.Data;
using System.Text;
using System.IO;
using EDBTypes;
using System.Collections.Generic;

namespace EnterpriseDB.EDBClient.Tests.EnterpriseDB
{
    /// <summary>
    /// Summary description for EDBByteaTest.
    /// </summary>

    [TestFixture]
    [NonParallelizable]
    public class EDBByteaTest : EPASTestBase
    {
        //String testImagePath = @"C:\Windows\System32\migwiz\PostMigRes\Web\base_images\AppInstalled.gif";
        const string testImagePath = @"C:\Windows\media\Windows Background.wav";

        [SetUp]
        public void Init()
        {
            using var conn = OpenConnection();                

            var com = new EDBCommand("", conn);
            com.CommandType = CommandType.Text;

            var strSqlEmptyArg = "create or replace procedure test_bytea_in_in(z in bytea,y in numeric) is declare begin null ;end;";
            com.CommandText = strSqlEmptyArg;
            com.ExecuteNonQuery();
            //			
            //			strSqlEmptyArg = "CREATE TABLE test_bytea_proc1( a numeric ,b bytea)";
            //			com.CommandText = strSqlEmptyArg;
            //			com.ExecuteNonQuery();

            strSqlEmptyArg = "CREATE TABLE IF NOT EXISTS test_bytea_two( a bytea ,b bytea)";
            com.CommandText = strSqlEmptyArg;
            com.ExecuteNonQuery();

            strSqlEmptyArg = "create or replace procedure test_bytea_two_in(z in bytea,y in bytea) is declare begin INSERT INTO test_bytea_two values(y,z);end;";
            com.CommandText = strSqlEmptyArg;
            com.ExecuteNonQuery();


            strSqlEmptyArg = "CREATE TABLE IF NOT EXISTS test_bytea_three( a bytea ,b bytea,c bytea);";
            com.CommandText = strSqlEmptyArg;
            com.ExecuteNonQuery();


            strSqlEmptyArg = "create or replace procedure test_bytea_three_in(z in bytea,y in bytea,x in bytea) is declare begin INSERT INTO test_bytea_three values(x,y,z);end;";
            com.CommandText = strSqlEmptyArg;
            com.ExecuteNonQuery();

            strSqlEmptyArg = "CREATE TABLE IF NOT EXISTS test_bytea_three_with_numeric( a bytea ,b bytea,c bytea,d  numeric);";
            com.CommandText = strSqlEmptyArg;
            com.ExecuteNonQuery();

            //				
            strSqlEmptyArg = "create or replace procedure test_bytea_three_in_with_numeric(z in bytea,y in bytea,x in bytea,xx numeric) is declare begin INSERT INTO test_bytea_three_with_numeric values(x,y,z,xx);end;";
            com.CommandText = strSqlEmptyArg;
            com.ExecuteNonQuery();


            strSqlEmptyArg = "create or replace procedure test_bytea_out(z out bytea) is declare begin select a into z from test_bytea_three_with_numeric ;end;";
            com.CommandText = strSqlEmptyArg;
            com.ExecuteNonQuery();


            strSqlEmptyArg = "create or replace procedure test_bytea_out_two(x out bytea,z out bytea) is declare begin select a into z from test_bytea_three_with_numeric ;select b into x from test_bytea_three_with_numeric ;end;";
            com.CommandText = strSqlEmptyArg;
            com.ExecuteNonQuery();



            strSqlEmptyArg = "create or replace procedure test_bytea_out_two_with_num(x out bytea,z out bytea,y in numeric ) is declare begin select a into z from test_bytea_three_with_numeric where d = y; select b into x from test_bytea_three_with_numeric where d = y; end;";
            com.CommandText = strSqlEmptyArg;
            com.ExecuteNonQuery();


            strSqlEmptyArg = "create or replace procedure test_bytea_out_two_with_num_varchar(x out bytea,z out bytea,y in numeric,xx in out  varchar ) is declare begin select a into z from test_bytea_three_with_numeric where d = y; select b into x from test_bytea_three_with_numeric where d = y;xx := 'EnterpriseDB';end;";
            com.CommandText = strSqlEmptyArg;
            com.ExecuteNonQuery();

            strSqlEmptyArg = "create or replace procedure test_bytea_out_two_with_numinout_varchar(x out bytea,z out bytea,y in out numeric,xx in out  varchar ) is declare begin select a into z from test_bytea_three_with_numeric where d = y;select b into x from test_bytea_three_with_numeric where d = y;xx := 'EnterpriseDB';y := 106;end;";
            com.CommandText = strSqlEmptyArg;
            com.ExecuteNonQuery();

            strSqlEmptyArg = "create or replace procedure test_bytea_inout_two_with_numinout_varchar(x out bytea,z out bytea,y in out numeric,xx in out  varchar ,yy out bytea) is declare begin select a into z from test_bytea_three_with_numeric where d = y;select b into x from test_bytea_three_with_numeric where d = y;xx := 'EnterpriseDB';y := 106;end;";
            com.CommandText = strSqlEmptyArg;
            com.ExecuteNonQuery();

            strSqlEmptyArg = "CREATE TABLE IF NOT EXISTS ByteaTest(id serial, f1 bytea);";
            com.CommandText = strSqlEmptyArg;
            com.ExecuteNonQuery();
        }

        [TearDown]
        public void Dispose()
        {
            using var conn = OpenConnection();

            var com = new EDBCommand("", conn);
            com.CommandType = CommandType.Text;

            com.CommandText = "DROP PROCEDURE test_bytea_inout_two_with_numinout_varchar";
            com.ExecuteNonQuery();


            com.CommandText = "DROP PROCEDURE test_bytea_out_two_with_numinout_varchar";
            com.ExecuteNonQuery();


            com.CommandText = "DROP PROCEDURE test_bytea_out_two_with_num_varchar";
            com.ExecuteNonQuery();


            com.CommandText = "DROP PROCEDURE test_bytea_out_two_with_num";
            com.ExecuteNonQuery();


            com.CommandText = "DROP PROCEDURE test_bytea_out_two";
            com.ExecuteNonQuery();


            com.CommandText = "DROP PROCEDURE test_bytea_out";
            com.ExecuteNonQuery();


            com.CommandText = "DROP PROCEDURE test_bytea_three_in_with_numeric";
            com.ExecuteNonQuery();

            com.CommandText = "DROP PROCEDURE test_bytea_three_in";
            com.ExecuteNonQuery();

            com.CommandText = "DROP PROCEDURE test_bytea_two_in";
            com.ExecuteNonQuery();


            com.CommandText = "DROP PROCEDURE test_bytea_in_in";
            com.ExecuteNonQuery();

            com.CommandText = "DROP TABLE test_bytea_three_with_numeric";
            com.ExecuteNonQuery();


            com.CommandText = "DROP TABLE test_bytea_two";
            com.ExecuteNonQuery();

            com.CommandText = "DROP TABLE test_bytea_three";
            com.ExecuteNonQuery();

            com.CommandText = "DROP TABLE ByteaTest";
            com.ExecuteNonQuery();


            if (conn.State != ConnectionState.Closed)
                conn.Close();
        }


        [Test]
        public void test_bytea_three_in_with_numeric()
        {
            try
            {
                using var conn = OpenConnection();
                using var fs = new FileStream(testImagePath, FileMode.Open, FileAccess.Read);
                var data = new byte[fs.Length];
                _ = fs.Read(data, 0, data.Length);
                fs.Close();

                var cmd = new EDBCommand("test_bytea_three_in_with_numeric(:imgin,:imgout,:img3,:num)", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add(new EDBParameter("imgin", EDBDbType.Bytea, 10000, "imgin", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, null!));
                cmd.Parameters.Add(new EDBParameter("imgout", EDBDbType.Bytea, 10000, "imgout", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, null!));
                cmd.Parameters.Add(new EDBParameter("img3", EDBDbType.Bytea, 10000, "img3", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, null!));
                cmd.Parameters.Add(new EDBParameter("num", EDBDbType.Numeric, 100, "num", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, null!));
                cmd.Prepare();
                cmd.Parameters[0].Value = data;
                cmd.Parameters[1].Value = data;
                cmd.Parameters[2].Value = data;
                cmd.Parameters[3].Value = 100;
                cmd.ExecuteNonQuery();

                Console.WriteLine("Image Saved");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        [Test]
        public void test_bytea_three_in()
        {
            try
            {
                using var conn = OpenConnection();
                using var fs = new FileStream(testImagePath, FileMode.Open, FileAccess.Read);
                var data = new byte[fs.Length];
                _ = fs.Read(data, 0, data.Length);
                fs.Close();

                var cmd = new EDBCommand("test_bytea_three_in(:imgin,:imgout,:img3)", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add(new EDBParameter("imgin", EDBDbType.Bytea, 10000, "imgin", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, null!));
                cmd.Parameters.Add(new EDBParameter("imgout", EDBDbType.Bytea, 10000, "imgout", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, null!));
                cmd.Parameters.Add(new EDBParameter("img3", EDBDbType.Bytea, 10000, "img3", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, null!));

                cmd.Prepare();
                cmd.Parameters[0].Value = data;
                cmd.Parameters[1].Value = data;
                cmd.Parameters[2].Value = data;
                cmd.ExecuteNonQuery();

                Console.WriteLine("Image Saved");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }

        [Test]
        public void test_bytea_in_in()
        {
            try
            {
                using var conn = OpenConnection();
                using var fs = new FileStream(testImagePath, FileMode.Open, FileAccess.Read);
                var data = new byte[fs.Length];
                _ = fs.Read(data, 0, data.Length);
                fs.Close();

                var cmd = new EDBCommand("test_bytea_in_in(:imgin,:imgout)", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add(new EDBParameter("imgin", EDBDbType.Bytea, 10000, "imgin", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, null!));
                cmd.Parameters.Add(new EDBParameter("imgout", EDBDbType.Numeric, 100, "imgout", ParameterDirection.Input, true, 2, 2, DataRowVersion.Current, 1));

                cmd.Prepare();
                cmd.Parameters[0].Value = data;
                cmd.Parameters[1].Value = 10;
                cmd.ExecuteNonQuery();

                Console.WriteLine("Image Saved");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }
        [Test]
        public void test_bytea_two_in()
        {

            try
            {
                using var conn = OpenConnection();
                using var fs = new FileStream(testImagePath, FileMode.Open, FileAccess.Read);
                var data = new byte[fs.Length];
                _ = fs.Read(data, 0, data.Length);
                fs.Close();

                var cmd = new EDBCommand("test_bytea_two_in(:imgin,:imgout)", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add(new EDBParameter("imgin", EDBDbType.Bytea, 10000, "imgin", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, null!));
                cmd.Parameters.Add(new EDBParameter("imgout", EDBDbType.Bytea, 10000, "imgout", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, null!));

                cmd.Prepare();
                cmd.Parameters[0].Value = data;
                cmd.Parameters[1].Value = data;
                cmd.ExecuteNonQuery();

                Console.WriteLine("Image Saved");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        static string EncodeHex(byte[] buf)
        {
            var hex = new StringBuilder(@"E'\\x", buf.Length * 2 + 3);
            foreach (var b in buf)
            {
                hex.Append(string.Format("{0:x2}", b));
            }
            hex.Append('\'');
            return hex.ToString();
        }

        [Test, Ignore("Needs refactor")]
        public void testa_bytea_out()
        {
            // Insert Data first
            using var conn = OpenConnection();
            using var fs_in = new FileStream(testImagePath, FileMode.Open, FileAccess.Read);
            var data = new byte[fs_in.Length];
            _ = fs_in.Read(data, 0, data.Length);
            fs_in.Close();
#pragma warning disable CS8604 // Possible null reference argument.
            conn.ExecuteNonQuery($"INSERT INTO test_bytea_three_with_numeric (a) VALUES ({EncodeHex(data)})");
#pragma warning restore CS8604 // Possible null reference argument.

            var cmd = new EDBCommand("test_bytea_out(:imgout)", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(new EDBParameter("imgout", EDBDbType.Bytea, 10000, "imgout", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, null!));
            cmd.Prepare();
            byte[] ss = { 1, 2, 3 };
            cmd.Parameters[0].Value = ss;
            var reader = cmd.ExecuteReader();
            reader.Read();
            Assert.True(reader.HasRows);
            if (reader.HasRows)
            {
                var image = new byte[Convert.ToInt32(reader.GetBytes(0, 0, null, 0, int.MaxValue))];
                reader.GetBytes(0, 0, image, 0, image.Length);
                Console.WriteLine("1");
                var fs = new
                    FileStream("C:\\edbtesting\\procout.gif", FileMode.Create, FileAccess.ReadWrite);

                for (var i = 0; i < image.Length; i++)
                    fs.WriteByte(image[i]);
                fs.Close();
            }
            while (reader.Read());

            Console.WriteLine("Image Saved");

        }

        [Test]
        public void testa_bytea_out_two()
        {
            /*try
			{
				EDBCommand cmd = new EDBCommand("test_bytea_out_two(:imgout,:imgout1)",conn);
				cmd.CommandType= CommandType.StoredProcedure;
				cmd.Parameters.Add(new EDBParameter("imgout", EDBTypes.EDBDbType.Bytea,10000,"imgout",ParameterDirection.Output,false ,2,2,System.Data.DataRowVersion.Current,null!));
				cmd.Parameters.Add(new EDBParameter("imgout1", EDBTypes.EDBDbType.Bytea,10000,"imgout1",ParameterDirection.Output,false ,2,2,System.Data.DataRowVersion.Current,null!));
				cmd.Prepare();
				//cmd.Parameters[0].Value = null;
				EDBDataReader reader = cmd.ExecuteReader();
				reader.Read(); 
				if (reader.HasRows) 
				{ 
					Byte[] image = new Byte[Convert.ToInt32((reader.GetBytes(0, 0,null, 0, Int32.MaxValue)))]; 
					reader.GetBytes(0, 0, image, 0, image.Length); 

					FileStream fs = new 
						FileStream("C:\\Temp\\procout1.gif", FileMode.Create, FileAccess.ReadWrite); 
				
					for(int i=0;i<image.Length;i++) 
						fs.WriteByte(image[i]); 
					fs.Close(); 
				}
                while (reader.Read()) ;
				//conn.Close();

				Console.WriteLine("Image Saved"); 
			}
			catch(Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}*/

        }
        [Test]
        public void testa_bytea_out_two_with_num()
        {
            /*try
			{
				EDBCommand cmd = new EDBCommand("test_bytea_out_two_with_num(:imgout,:imgout1,:num)",conn);
				cmd.CommandType= CommandType.StoredProcedure;
				cmd.Parameters.Add(new EDBParameter("imgout", EDBTypes.EDBDbType.Bytea,10000,"imgout",ParameterDirection.Output,false ,2,2,System.Data.DataRowVersion.Current,null!));
				cmd.Parameters.Add(new EDBParameter("imgout1", EDBTypes.EDBDbType.Bytea,10000,"imgout1",ParameterDirection.Output,false ,2,2,System.Data.DataRowVersion.Current,null!));
				cmd.Parameters.Add(new EDBParameter("num", EDBTypes.EDBDbType.Numeric,10,"num",ParameterDirection.Input,false ,2,2,System.Data.DataRowVersion.Current,null!));
				cmd.Prepare();
				cmd.Parameters[2].Value = 100;
				EDBDataReader reader = cmd.ExecuteReader();
				reader.Read(); 
				if (reader.HasRows) 
				{ 
					Byte[] image = new Byte[Convert.ToInt32((reader.GetBytes(0, 0,null, 0, Int32.MaxValue)))]; 
					reader.GetBytes(0, 0, image, 0, image.Length); 

					FileStream fs = new 
						FileStream("C:\\Temp\\procout11.gif", FileMode.Create, FileAccess.ReadWrite); 
				
					for(int i=0;i<image.Length;i++) 
						fs.WriteByte(image[i]); 
					fs.Close(); 
				
					Byte[] image1 = new Byte[Convert.ToInt32((reader.GetBytes(1, 0,null, 0, Int32.MaxValue)))]; 
					reader.GetBytes(1, 0, image1, 0, image1.Length); 

					FileStream fs1 = new 
						FileStream("C:\\Temp\\procout12.gif", FileMode.Create, FileAccess.ReadWrite); 
				
					for(int i=0;i<image1.Length;i++) 
						fs1.WriteByte(image[i]); 
					fs1.Close(); 
				
				
				
				}
                while (reader.Read()) ;
				//conn.Close();

				Console.WriteLine("Image Saved"); 
			}
			catch(Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}*/

        }

        [Test]
        public void testa_bytea_out_two_with_num_varchar()
        {


            /*	try
                {
                    EDBCommand cmd = new EDBCommand("test_bytea_out_two_with_num_varchar(:imgout,:imgout1,:num,:var)",conn);
                    cmd.CommandType= CommandType.StoredProcedure;
                    cmd.Parameters.Add(new EDBParameter("imgout", EDBTypes.EDBDbType.Bytea,10000,"imgout",ParameterDirection.Output,false ,2,2,System.Data.DataRowVersion.Current,null!));
                    cmd.Parameters.Add(new EDBParameter("imgout1", EDBTypes.EDBDbType.Bytea,10000,"imgout1",ParameterDirection.Output,false ,2,2,System.Data.DataRowVersion.Current,null!));
                    cmd.Parameters.Add(new EDBParameter("num", EDBTypes.EDBDbType.Numeric,10,"num",ParameterDirection.Input,false ,2,2,System.Data.DataRowVersion.Current,null!));
                    cmd.Parameters.Add(new EDBParameter("var", EDBTypes.EDBDbType.Varchar,10,"var",ParameterDirection.InputOutput,false ,2,2,System.Data.DataRowVersion.Current,null!));
                    cmd.Prepare();
                    cmd.Parameters[2].Value = 100;
                    EDBDataReader reader = cmd.ExecuteReader();
                    reader.Read(); 
                    if (reader.HasRows) 
                    { 
                        Byte[] image = new Byte[Convert.ToInt32((reader.GetBytes(0, 0,null, 0, Int32.MaxValue)))]; 
                        reader.GetBytes(0, 0, image, 0, image.Length); 

                        FileStream fs = new 
                            FileStream("C:\\Temp\\procout11.gif", FileMode.Create, FileAccess.ReadWrite); 

                        for(int i=0;i<image.Length;i++) 
                            fs.WriteByte(image[i]); 
                        fs.Close(); 

                        Byte[] image1 = new Byte[Convert.ToInt32((reader.GetBytes(1, 0,null, 0, Int32.MaxValue)))]; 
                        reader.GetBytes(1, 0, image1, 0, image1.Length); 

                        FileStream fs1 = new 
                            FileStream("C:\\Temp\\procout12.gif", FileMode.Create, FileAccess.ReadWrite); 

                        for(int i=0;i<image1.Length;i++) 
                            fs1.WriteByte(image[i]); 
                        fs1.Close(); 

                        Console.WriteLine(cmd.Parameters[3].Value.ToString());

                    }
                    while (reader.Read()) ;
    //				conn.Close();

                    Console.WriteLine("Image Saved"); 
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            */
        }
        public void testa_bytea_out_two_with_num_varchar1()
        {

            try
            {
                using var conn = OpenConnection();
                var cmd = new EDBCommand("test_bytea_out_two_with_num_varchar(:imgout,:imgout1,:num,:var)", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new EDBParameter("imgout", EDBDbType.Bytea, 10000, "imgout", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, null!));
                cmd.Parameters.Add(new EDBParameter("imgout1", EDBDbType.Bytea, 10000, "imgout1", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, null!));
                cmd.Parameters.Add(new EDBParameter("num", EDBDbType.Numeric, 10, "num", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, null!));
                cmd.Parameters.Add(new EDBParameter("var", EDBDbType.Varchar, 10, "var", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, null!));
                cmd.Prepare();
                cmd.Parameters[2].Value = 100;
                var reader = cmd.ExecuteReader();
                reader.Read();
                if (reader.HasRows)
                {
                    var image = new byte[Convert.ToInt32(reader.GetBytes(0, 0, null, 0, int.MaxValue))];
                    reader.GetBytes(0, 0, image, 0, image.Length);

                    var fs = new
                        FileStream("C:\\Temp\\procout11.gif", FileMode.Create, FileAccess.ReadWrite);

                    for (var i = 0; i < image.Length; i++)
                        fs.WriteByte(image[i]);
                    fs.Close();

                    var image1 = new byte[Convert.ToInt32(reader.GetBytes(1, 0, null, 0, int.MaxValue))];
                    reader.GetBytes(1, 0, image1, 0, image1.Length);

                    var fs1 = new
                        FileStream("C:\\Temp\\procout12.gif", FileMode.Create, FileAccess.ReadWrite);

                    for (var i = 0; i < image1.Length; i++)
                        fs1.WriteByte(image[i]);
                    fs1.Close();

                    Console.WriteLine(cmd.Parameters[3].Value!.ToString());
                    Console.WriteLine(cmd.Parameters[2].Value!.ToString());
                    while (reader.Read()) ;
                    var commd = new EDBCommand("DROP TABLE test_bytea_three_with_numeric", conn);

                    commd.ExecuteNonQuery();
                }


                Console.WriteLine("Image Saved");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }
        public void testa_bytea_inout_two_with_numinout_varchar()
        {
            try
            {
                using var conn = OpenConnection();
                var cmd = new EDBCommand("test_bytea_inout_two_with_numinout_varchar(:imgout,:imgout1,:num,:var,:imgout2)", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new EDBParameter("imgout", EDBDbType.Bytea, 10000, "imgout", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, null!));
                cmd.Parameters.Add(new EDBParameter("imgout1", EDBDbType.Bytea, 10000, "imgout1", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, null!));
                cmd.Parameters.Add(new EDBParameter("num", EDBDbType.Numeric, 10, "num", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, null!));
                cmd.Parameters.Add(new EDBParameter("var", EDBDbType.Varchar, 10, "var", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, null!));
                cmd.Parameters.Add(new EDBParameter("imgout2", EDBDbType.Bytea, 10, "imgout2", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, null!));
                cmd.Prepare();
                cmd.Parameters[2].Value = 100;
                var reader = cmd.ExecuteReader();
                reader.Read();
                if (reader.HasRows)
                {
                    var image = new byte[Convert.ToInt32(reader.GetBytes(0, 0, null, 0, int.MaxValue))];
                    reader.GetBytes(0, 0, image, 0, image.Length);

                    var fs = new
                        FileStream("C:\\Temp\\procout11.gif", FileMode.Create, FileAccess.ReadWrite);

                    for (var i = 0; i < image.Length; i++)
                        fs.WriteByte(image[i]);
                    fs.Close();

                    var image1 = new byte[Convert.ToInt32(reader.GetBytes(1, 0, null, 0, int.MaxValue))];
                    reader.GetBytes(1, 0, image1, 0, image1.Length);

                    var fs1 = new
                        FileStream("C:\\Temp\\procout12.gif", FileMode.Create, FileAccess.ReadWrite);

                    for (var i = 0; i < image1.Length; i++)
                        fs1.WriteByte(image[i]);
                    fs1.Close();

                    Console.WriteLine(cmd.Parameters[3].Value!.ToString());
                    Console.WriteLine(cmd.Parameters[2].Value!.ToString());
                }
                while (reader.Read()) ;
                //				conn.Close();

                Console.WriteLine("Image Saved");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }

        [Test]
        public void CRUDTest()
        {
            using var conn = OpenConnection();
            byte[] data = { 1, 23, 3 };
            byte[] data2 = { 1, 3, 4 };

            var command = new EDBCommand("INSERT INTO ByteaTest Values(1, :data)", conn);
            command.Parameters.Add(new EDBParameter("data", EDBDbType.Bytea));
            command.Parameters[0].Value = data;

            var rowsAdded = command.ExecuteNonQuery();
            Assert.AreEqual(1, rowsAdded);

            // Retrieve
            command = new EDBCommand("select f1 from ByteaTest;", conn);
            Assert.IsInstanceOf<byte[]>(command.ExecuteScalar());

            // Update
            command = new EDBCommand("Update ByteaTest set f1 = :b where id = 1", conn);
            command.Parameters.Add(new EDBParameter("b", EDBDbType.Bytea));
            command.Parameters[0].Value = data2;

            rowsAdded = command.ExecuteNonQuery();
            Assert.AreEqual(1, rowsAdded);

            command = new EDBCommand("select f1 from ByteaTest;", conn);
            Assert.IsInstanceOf<byte[]>(command.ExecuteScalar());

            // Delete
            command = new EDBCommand("Delete from ByteaTest where id = 1", conn);

            rowsAdded = command.ExecuteNonQuery();
            Assert.AreEqual(1, rowsAdded);

            command = new EDBCommand("select f1 from ByteaTest;", conn);
            Assert.AreEqual(-1, command.ExecuteNonQuery());
        }


    }

}
