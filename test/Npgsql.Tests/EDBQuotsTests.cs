#region License
// The PostgreSQL License
//
// Copyright (C) 2017 The EDB Development Team
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
//
// IN NO EVENT SHALL THE EDB DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE EDB DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// THE EDB DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE EDB DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.
#endregion

using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using EnterpriseDB.EDBClient;
using NUnit.Framework;
using System.Data;


namespace EnterpriseDB.EDBClient.Tests
{
#pragma warning disable CS8600
#pragma warning disable CS8602
    [TestFixture]
    public class EDBQuotsTests : TestBase
    {
        private EDBConnection? _conn = null;

        #region Setup / Tear Down
        [SetUp]
        protected void SetUp()
        {
            _conn = new EDBConnection(ConnectionString);

        }

        [TearDown]
        protected void TearDown()
        {
            if (_conn.State != ConnectionState.Closed)
                _conn.Close();
        }
        #endregion


        [Test]
        public void QuoteHandling1()
        {
            _conn.Open();

            EDBCommand com = new EDBCommand("", _conn);
            com.CommandText = "select empno from emp where ename= :Name";
            com.Parameters.Add(new EDBParameter("Name", EDBTypes.EDBDbType.Varchar));
            com.Parameters[0].Value = "SMITH";
            EDBDataReader Reader = com.ExecuteReader();

            while (Reader.Read())
            {
                Console.WriteLine(Reader.GetDecimal(0));
            }
            Reader.Close();
            _conn.Close();
        }

        [Test]
        public void QuoteHandling2()
        {
            _conn.Open();

            EDBCommand com = new EDBCommand("", _conn);
            com.CommandText = "CREATE TABLE Quote(id int4, b char)";
            com.ExecuteNonQuery();
            com.CommandText = "INSERT INTO Quote values(1, 't')";
            com.ExecuteNonQuery();
            com.CommandText = "select id from Quote where b= :No";
            com.Parameters.Add(new EDBParameter("No", EDBTypes.EDBDbType.Varchar));
            com.Parameters[0].Value = "t";
            EDBDataReader Reader = com.ExecuteReader();

            while (Reader.Read())
            {
                Console.WriteLine(Reader.GetInt32(0));
            }
            Reader.Close();
            com.CommandText = "DROP TABLE Quote";
            com.ExecuteNonQuery();
            _conn.Close();
        }

        [Test]
        public void QuoteHandling3()
        {
            _conn.Open();

            EDBCommand com = new EDBCommand("", _conn);
            com.CommandText = "select empno from emp where ename= :Name";
            com.Parameters.Add(new EDBParameter("Name", EDBTypes.EDBDbType.Varchar, 4));
            com.Parameters[0].Value = "SMITH";
            EDBDataReader Reader = com.ExecuteReader();

            while (Reader.Read())
            {
                Console.WriteLine(Reader.GetInt32(0));
            }
            Reader.Close();
            _conn.Close();
        }

        [Test]
        public void QuoteHandling4()
        {
            _conn.Open();

            EDBCommand com = new EDBCommand("", _conn);
            com.CommandText = "CREATE TABLE Quote(id int4, b char)";
            com.ExecuteNonQuery();
            com.CommandText = "INSERT INTO Quote values(1, 't')";
            com.ExecuteNonQuery();
            com.CommandText = "select id from Quote where b= :No";
            com.Parameters.Add(new EDBParameter("No", EDBTypes.EDBDbType.Varchar, 1));
            com.Parameters[0].Value = "t";
            EDBDataReader Reader = com.ExecuteReader();

            while (Reader.Read())
            {
                Console.WriteLine(Reader.GetInt32(0));
            }
            Reader.Close();
            com.CommandText = "DROP TABLE Quote";
            com.ExecuteNonQuery();
            _conn.Close();
        }

        [Test]
        public void QuoteHandling5()
        {
            _conn.Open();

            EDBCommand com = new EDBCommand("", _conn);
            com.CommandText = "select empno from emp where ename= :Name";
            com.Parameters.Add(new EDBParameter("Name", EDBTypes.EDBDbType.Char, 5));
            com.Parameters[0].Value = "SMITH";
            EDBDataReader Reader = com.ExecuteReader();

            while (Reader.Read())
            {
                Console.WriteLine(Reader.GetDecimal(0));

            }
            Reader.Close();
            _conn.Close();
        }

        [Test]
        public void QuoteHandling6()
        {
            _conn.Open();

            EDBCommand com = new EDBCommand("", _conn);
            com.CommandText = "CREATE TABLE Quote(id int4, b char)";
            com.ExecuteNonQuery();
            com.CommandText = "INSERT INTO Quote values(1, 't')";
            com.ExecuteNonQuery();
            EDBDataReader? Reader = null;
            com.CommandText = "select id from Quote where b= :No";


            com.Parameters.Add(new EDBParameter("No", EDBTypes.EDBDbType.Char, 1));
            com.Parameters[0].Value = "t";
            Reader = com.ExecuteReader();



            while (Reader.Read())
            {
                Console.WriteLine(Reader.GetInt32(0));
            }
            Reader.Close();
            com.CommandText = "DROP TABLE Quote";
            com.ExecuteNonQuery();
            _conn.Close();
        }


        [Test]
        public void QuoteHandling7()
        {
            _conn.Open();

            EDBCommand com = new EDBCommand("", _conn);
            com.CommandText = "CREATE TABLE Quote(id int4, b char)";
            com.ExecuteNonQuery();
            com.CommandText = "INSERT INTO Quote values(1, 't')";
            com.ExecuteNonQuery();
            EDBDataReader? Reader = null;
            com.CommandText = "select id from Quote where b= :No";
            try
            {
                com.Parameters.Add(new EDBParameter("No", EDBTypes.EDBDbType.Char));
                com.Parameters[0].Value = "t";
                Reader = com.ExecuteReader();


            }

            catch (EDBException)
            {
                //Console.WriteLine(exp.Message);
                com.CommandText = "DROP TABLE Quote";
                com.ExecuteNonQuery();
                _conn.Close();
                return;
            }

            while (Reader.Read())
            {
                Console.WriteLine(Reader.GetInt32(0));
            }
            Reader.Close();
            com.CommandText = "DROP TABLE Quote";
            com.ExecuteNonQuery();
            _conn.Close();
        }

        [Test]
        public void QuoteHandling8()
        {
            _conn.Open();

            EDBCommand com = new EDBCommand("", _conn);
            com.CommandText = "CREATE TABLE Quote(id int4, b char)";
            com.ExecuteNonQuery();
            com.CommandText = "INSERT INTO Quote values(1, 't')";
            com.ExecuteNonQuery();
            EDBDataReader? Reader = null;
            com.CommandText = "select id from Quote where b= :No";
            try
            {
                com.Parameters.Add(new EDBParameter("No", EDBTypes.EDBDbType.Varchar));
                com.Parameters[0].Value = "t";
                Reader = com.ExecuteReader();

            }

            catch (EDBException)
            {
                //Console.WriteLine(exp.Message);
                com.CommandText = "DROP TABLE Quote";
                com.ExecuteNonQuery();
                _conn.Close();
                return;
            }

            while (Reader.Read())
            {
                Console.WriteLine(Reader.GetInt32(0));
            }
            Reader.Close();
            com.CommandText = "DROP TABLE Quote";
            com.ExecuteNonQuery();
            _conn.Close();
        }

        [Test]
        public void QuoteHandling9()
        {
            _conn.Open();

            EDBCommand com = new EDBCommand("", _conn);
            com.CommandText = "CREATE TABLE Quote(id int4, b char)";
            com.ExecuteNonQuery();
            com.CommandText = "INSERT INTO Quote values(1, 't')";
            com.ExecuteNonQuery();
            EDBDataReader? Reader = null;
            com.CommandText = "select id from Quote where b= :No";
            try
            {
                com.Parameters.Add(new EDBParameter("No", EDBTypes.EDBDbType.Char, -1));
                com.Parameters[0].Value = "t";
                Reader = com.ExecuteReader();


                //Assert.Fail("should fail for negative value of size");
            }

            catch (EDBException)
            {
                //Console.WriteLine(exp.Message);
                com.CommandText = "DROP TABLE Quote";
                com.ExecuteNonQuery();
                _conn.Close();
                return;
            }

            while (Reader.Read())
            {
                Console.WriteLine(Reader.GetInt32(0));
            }
            Reader.Close();
            com.CommandText = "DROP TABLE Quote";
            com.ExecuteNonQuery();
            _conn.Close();
        }

        [Test]
        public void QuoteHandling10()
        {
            _conn.Open();

            EDBCommand com = new EDBCommand("", _conn);
            com.CommandText = "select empno from emp where ename= :Name";
            com.Parameters.Add(new EDBParameter("Name", EDBTypes.EDBDbType.Char, -4));
            com.Parameters[0].Value = "SMITH";
            EDBDataReader Reader = null;
            try
            {
                Reader = com.ExecuteReader();

            }

            catch (EDBException)
            {
                _conn.Close();
                return;
            }
            while (Reader.Read())
            {
                Console.WriteLine(Reader.GetInt32(0));
            }
            Reader.Close();
            _conn.Close();
        }


        [Test]
        public void QuoteHandling11()
        {
            _conn.Open();

            EDBCommand com = new EDBCommand("", _conn);
            com.CommandText = "select empno from emp where ename= :Name";
            com.Parameters.Add(new EDBParameter("Name", EDBTypes.EDBDbType.Varchar, -4));
            com.Parameters[0].Value = "SMITH";
            EDBDataReader Reader = com.ExecuteReader();

            while (Reader.Read())
            {
                Console.WriteLine(Reader.GetDecimal(0));
            }
            Reader.Close();
            _conn.Close();
        }

        [Test]
        public void QuoteHandling12()
        {
            _conn.Open();

            EDBCommand com = new EDBCommand("", _conn);
            com.CommandText = "CREATE TABLE Quote(id int4, b char)";
            com.ExecuteNonQuery();
            com.CommandText = "INSERT INTO Quote values(1, 't')";
            com.ExecuteNonQuery();
            com.CommandText = "select id from Quote where b= :No";
            com.Parameters.Add(new EDBParameter("No", EDBTypes.EDBDbType.Varchar, -1));
            com.Parameters[0].Value = "t";
            EDBDataReader Reader = com.ExecuteReader();

            while (Reader.Read())
            {
                Console.WriteLine(Reader.GetInt32(0));
            }
            Reader.Close();
            com.CommandText = "DROP TABLE Quote";
            com.ExecuteNonQuery();
            _conn.Close();
        }


        [Test]
        public void QuoteHandling13()
        {
            _conn.Open();

            EDBCommand com = new EDBCommand("", _conn);
            com.CommandText = "select empno from emp where ename= :Name";
            com.Parameters.Add(new EDBParameter("Name", EDBTypes.EDBDbType.Varchar, -4));
            com.Parameters[0].Value = "SMITH";
            EDBDataReader Reader = null;
            try
            {
                Reader = com.ExecuteReader();

            }

            catch (Exception)
            {
                return;
            }


            while (Reader.Read())
            {
                Console.WriteLine(Reader.GetDecimal(0));
            }
            Reader.Close();
            _conn.Close();
        }

        [Test]
        public void QuoteHandling14()
        {
            _conn.Open();

            EDBCommand com = new EDBCommand("", _conn);
            com.CommandText = "CREATE TABLE Quote(id int4, b char)";
            com.ExecuteNonQuery();
            com.CommandText = "INSERT INTO Quote values(1, 't')";
            com.ExecuteNonQuery();
            com.CommandText = "select id from Quote where b= :No";
            com.Parameters.Add(new EDBParameter("No", EDBTypes.EDBDbType.Varchar, -1));
            com.Parameters[0].Value = "t";
            EDBDataReader Reader = null;
            try
            {
                Reader = com.ExecuteReader();

            }

            catch (Exception)
            {
                com.CommandText = "DROP TABLE Quote";
                com.ExecuteNonQuery();

                return;
            }

            while (Reader.Read())
            {
                Console.WriteLine(Reader.GetInt32(0));
            }
            Reader.Close();
            com.CommandText = "DROP TABLE Quote";
            com.ExecuteNonQuery();
            _conn.Close();
        }


        [Test]
        public void QuoteHandling15()
        {
            _conn.Open();

            EDBCommand com = new EDBCommand("", _conn);
            com.CommandText = "CREATE TABLE Quote(id int4, b char)";
            com.ExecuteNonQuery();
            com.CommandText = "INSERT INTO Quote values(1, 't')";
            com.ExecuteNonQuery();
            EDBDataReader Reader = null;
            com.CommandText = "select id from Quote where b= :No";
            try
            {
                com.Parameters.Add(new EDBParameter("No", EDBTypes.EDBDbType.Char, 0));
                com.Parameters[0].Value = "t";
                Reader = com.ExecuteReader();


                //Assert.Fail("should fail for negative value of size");
            }

            catch (EDBException)
            {
                //Console.WriteLine(exp.Message);
                com.CommandText = "DROP TABLE Quote";
                com.ExecuteNonQuery();
                _conn.Close();
                return;
            }

            while (Reader.Read())
            {
                Console.WriteLine(Reader.GetInt32(0));
            }
            Reader.Close();
            com.CommandText = "DROP TABLE Quote";
            com.ExecuteNonQuery();
            _conn.Close();
        }

        [Test]
        public void QuoteHandling16()
        {
            _conn.Open();

            EDBCommand com = new EDBCommand("", _conn);
            com.CommandText = "select empno from emp where ename= :Name";
            com.Parameters.Add(new EDBParameter("Name", EDBTypes.EDBDbType.Char, 4));
            com.Parameters[0].Value = "SMITH";
            EDBDataReader Reader = null;
            try
            {
                Reader = com.ExecuteReader();

            }

            catch (EDBException)
            {
                _conn.Close();
                return;
            }
            while (Reader.Read())
            {
                Console.WriteLine(Reader.GetInt32(0));
            }
            Reader.Close();
            _conn.Close();
        }


        [Test]
        public void QuoteHandling17()
        {
            _conn.Open();

            EDBCommand com = new EDBCommand("", _conn);
            com.CommandText = "select empno from emp where ename= :Name";
            com.Parameters.Add(new EDBParameter("Name", EDBTypes.EDBDbType.Varchar, 4));
            com.Parameters[0].Value = "SMITH";
            EDBDataReader Reader = com.ExecuteReader();

            while (Reader.Read())
            {
                Console.WriteLine(Reader.GetDecimal(0));
            }
            Reader.Close();
            _conn.Close();
        }

        [Test]
        public void QuoteHandling18()
        {
            _conn.Open();

            EDBCommand com = new EDBCommand("", _conn);
            com.CommandText = "CREATE TABLE Quote(id int4, b char)";
            com.ExecuteNonQuery();
            com.CommandText = "INSERT INTO Quote values(1, 't')";
            com.ExecuteNonQuery();
            com.CommandText = "select id from Quote where b= :No";
            com.Parameters.Add(new EDBParameter("No", EDBTypes.EDBDbType.Varchar, 0));
            com.Parameters[0].Value = "t";
            EDBDataReader Reader = com.ExecuteReader();

            while (Reader.Read())
            {
                Console.WriteLine(Reader.GetInt32(0));
            }
            Reader.Close();
            com.CommandText = "DROP TABLE Quote";
            com.ExecuteNonQuery();
            _conn.Close();
        }


        [Test]
        public void QuoteHandling19()
        {
            _conn.Open();

            EDBCommand com = new EDBCommand("", _conn);
            com.CommandText = "select empno from emp where ename= :Name";
            com.Parameters.Add(new EDBParameter("Name", EDBTypes.EDBDbType.Varchar, 4));
            com.Parameters[0].Value = "SMITH";
            EDBDataReader Reader = null;
            try
            {
                Reader = com.ExecuteReader();

            }

            catch (Exception)
            {
                return;
            }


            while (Reader.Read())
            {
                Console.WriteLine(Reader.GetInt32(0));
            }
            Reader.Close();
            _conn.Close();
        }

        [Test]
        public void QuoteHandling20()
        {
            _conn.Open();

            EDBCommand com = new EDBCommand("", _conn);
            com.CommandText = "CREATE TABLE Quote(id int4, b char)";
            com.ExecuteNonQuery();
            com.CommandText = "INSERT INTO Quote values(1, 't')";
            com.ExecuteNonQuery();
            com.CommandText = "select id from Quote where b= :No";
            com.Parameters.Add(new EDBParameter("No", EDBTypes.EDBDbType.Varchar, 0));
            com.Parameters[0].Value = "t";
            EDBDataReader Reader = null;
            try
            {
                Reader = com.ExecuteReader();

            }

            catch (Exception)
            {
                com.CommandText = "DROP TABLE Quote";
                com.ExecuteNonQuery();

                return;
            }

            while (Reader.Read())
            {
                Console.WriteLine(Reader.GetInt32(0));
            }
            Reader.Close();
            com.CommandText = "DROP TABLE Quote";
            com.ExecuteNonQuery();
            _conn.Close();
        }

    }
#pragma warning restore CS8600
#pragma warning restore CS8602
}
