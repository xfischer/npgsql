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
using System.IO;
using System.Globalization;


using System.Net;
using System.Net.Sockets;
using EDBTypes;
using System.Resources;
using System.Threading;
using System.Reflection;
using System.Text;
using NUnit.Framework.Constraints;

namespace EnterpriseDB.EDBClient.Tests
{
#pragma warning disable CS8602
    [TestFixture]
    public class EDBInsertTests : TestBase
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
        public void InsertDoubleValue()
        {
            _conn.Open();


            EDBCommand command = new EDBCommand("INSERT INTO tabled(field_float8) values (:a)", _conn);
            command.Parameters.Add(new EDBParameter(":a", DbType.Double));

            command.Parameters[0].Value = 7.4D;

            Int32 rowsAdded = command.ExecuteNonQuery();

            Assert.AreEqual(1, rowsAdded);

            command.CommandText = "select * from tabled where field_float8 = :a";


            EDBDataReader dr = command.ExecuteReader();
            dr.Read();

            Double result = dr.GetDouble(2);
            dr.Close();

            command.CommandText = "delete from tabled where field_serial > 2;";
            command.Parameters.Clear();
            //command.ExecuteNonQuery();


            Assert.AreEqual(7.4D, result);

        }


        [Test]
        public void InsertDoubleValueEDBDbType()
        {
            _conn.Open();

            EDBCommand command = new EDBCommand("INSERT INTO tabled(field_float8) values (:a)", _conn);
            command.Parameters.Add(new EDBParameter(":a", EDBDbType.Double));

            command.Parameters[0].Value = 7.4D;

            Int32 rowsAdded = command.ExecuteNonQuery();

            Assert.AreEqual(1, rowsAdded);

            command.CommandText = "select * from tabled where field_float8 = :a";
            
            EDBDataReader dr = command.ExecuteReader();
            dr.Read();

            Double result = dr.GetDouble(2);
            dr.Close();
            
            command.CommandText = "delete from tabled where field_serial > 2;";
            command.Parameters.Clear();
            //command.ExecuteNonQuery();

            Assert.AreEqual(7.4D, result);

        }

        [Test]
        public void InsertNullString()
        {
            _conn.Open();

            EDBCommand command = new EDBCommand("INSERT INTO tablea(field_text) values (:a)", _conn);

            command.Parameters.Add(new EDBParameter("a", DbType.String));

            command.Parameters[0].Value = DBNull.Value;

            Int32 rowsAdded = command.ExecuteNonQuery();

            Assert.AreEqual(1, rowsAdded);

            command.CommandText = "select count(*) from tablea where field_text is null";
            command.Parameters.Clear();

            long result = (long)command.ExecuteScalar();

            command.CommandText = "delete from tablea where field_serial = (select max(field_serial) from tablea) and field_serial != 4;";
            command.ExecuteNonQuery();

            Assert.AreEqual(4, result);



        }

        [Test]
        public void InsertNullStringEDBDbType()
        {
            _conn.Open();

            EDBCommand command = new EDBCommand("INSERT INTO tablea(field_text) values (:a)", _conn);

            command.Parameters.Add(new EDBParameter("a", EDBDbType.Text));

            command.Parameters[0].Value = DBNull.Value;

            Int32 rowsAdded = command.ExecuteNonQuery();

            Assert.AreEqual(1, rowsAdded);

            command.CommandText = "select count(*) from tablea where field_text is null";
            command.Parameters.Clear();

            long result = (long)command.ExecuteScalar();

            command.CommandText = "delete from tablea where field_serial = (select max(field_serial) from tablea) and field_serial != 4;";
            command.ExecuteNonQuery();

            Assert.AreEqual(4, result);



        }



        [Test]
        public void InsertNullDateTime()
        {
            _conn.Open();

            EDBCommand command = new EDBCommand("INSERT INTO tableb(field_timestamp) values (:a)", _conn);

            command.Parameters.Add(new EDBParameter("a", DbType.DateTime));

            command.Parameters[0].Value = DBNull.Value;

            Int32 rowsAdded = command.ExecuteNonQuery();

            Assert.AreEqual(1, rowsAdded);

            command.CommandText = "select count(*) from tableb where field_timestamp is null";
            command.Parameters.Clear();

            object result = command.ExecuteScalar();

            command.CommandText = "delete from tableb where field_serial = (select max(field_serial) from tableb) and field_serial != 3;";
            command.ExecuteNonQuery();

            Assert.AreEqual(4, result);



        }


        [Test]
        public void InsertNullDateTimeEDBDbType()
        {
            _conn.Open();

            EDBCommand command = new EDBCommand("INSERT INTO tableb(field_timestamp) values (:a)", _conn);

            command.Parameters.Add(new EDBParameter("a", EDBDbType.Timestamp));

            command.Parameters[0].Value = DBNull.Value;

            Int32 rowsAdded = command.ExecuteNonQuery();

            Assert.AreEqual(1, rowsAdded);

            command.CommandText = "select count(*) from tableb where field_timestamp is null";
            command.Parameters.Clear();

            object result = command.ExecuteScalar();

            command.CommandText = "delete from tableb where field_serial = (select max(field_serial) from tableb) and field_serial != 3;";
            command.ExecuteNonQuery();

            Assert.AreEqual(4, result);



        }



        [Test]
        public void InsertNullInt16()
        {
            _conn.Open();


            EDBCommand command = new EDBCommand("INSERT INTO tableb(field_int2) values (:a)", _conn);

            command.Parameters.Add(new EDBParameter("a", DbType.Int16));

            command.Parameters[0].Value = DBNull.Value;

            Int32 rowsAdded = command.ExecuteNonQuery();

            Assert.AreEqual(1, rowsAdded);

            command.CommandText = "select count(*) from tableb where field_int2 is null";
            command.Parameters.Clear();

            object result = command.ExecuteScalar(); // The missed cast is needed as Server7.2 returns Int32 and Server7.3+ returns Int64

            command.CommandText = "delete from tableb where field_serial = (select max(field_serial) from tableb);";
            command.ExecuteNonQuery();

            Assert.AreEqual(4, result);


        }


        [Test]
        public void InsertNullInt16EDBDbType()
        {
            _conn.Open();


            EDBCommand command = new EDBCommand("INSERT INTO tableb(field_int2) values (:a)", _conn);

            command.Parameters.Add(new EDBParameter("a", EDBDbType.Smallint));

            command.Parameters[0].Value = DBNull.Value;

            Int32 rowsAdded = command.ExecuteNonQuery();

            Assert.AreEqual(1, rowsAdded);

            command.CommandText = "select count(*) from tableb where field_int2 is null";
            command.Parameters.Clear();

            object result = command.ExecuteScalar(); // The missed cast is needed as Server7.2 returns Int32 and Server7.3+ returns Int64

            command.CommandText = "delete from tableb where field_serial = (select max(field_serial) from tableb);";
            command.ExecuteNonQuery();

            Assert.AreEqual(4, result);


        }


        [Test]
        public void InsertNullInt32()
        {
            _conn.Open();


            EDBCommand command = new EDBCommand("INSERT INTO tablea(field_int4) values (:a)", _conn);

            command.Parameters.Add(new EDBParameter("a", DbType.Int32));

            command.Parameters[0].Value = DBNull.Value;

            Int32 rowsAdded = command.ExecuteNonQuery();

            Assert.AreEqual(1, rowsAdded);

            command.CommandText = "select count(*) from tablea where field_int4 is null";
            command.Parameters.Clear();

            object result = command.ExecuteScalar(); // The missed cast is needed as Server7.2 returns Int32 and Server7.3+ returns Int64

            command.CommandText = "delete from tablea where field_serial = (select max(field_serial) from tablea);";
            command.ExecuteNonQuery();

            Assert.AreEqual(5, result);

        }


        [Test]
        public void InsertNullNumeric()
        {
            _conn.Open();


            EDBCommand command = new EDBCommand("INSERT INTO tableb(field_numeric) values (:a)", _conn);

            command.Parameters.Add(new EDBParameter("a", DbType.Decimal));

            command.Parameters[0].Value = DBNull.Value;

            Int32 rowsAdded = command.ExecuteNonQuery();

            Assert.AreEqual(1, rowsAdded);

            command.CommandText = "select count(*) from tableb where field_numeric is null";
            command.Parameters.Clear();

            object result = command.ExecuteScalar(); // The missed cast is needed as Server7.2 returns Int32 and Server7.3+ returns Int64

            command.CommandText = "delete from tableb where field_serial = (select max(field_serial) from tableb);";
            command.ExecuteNonQuery();

            Assert.AreEqual(3, result);

        }

        [Test]
        public void InsertNullBoolean()
        {
            _conn.Open();


            EDBCommand command = new EDBCommand("INSERT INTO tablea(field_bool) values (:a)", _conn);

            command.Parameters.Add(new EDBParameter("a", DbType.Boolean));

            command.Parameters[0].Value = DBNull.Value;

            Int32 rowsAdded = command.ExecuteNonQuery();

            Assert.AreEqual(1, rowsAdded);

            command.CommandText = "select count(*) from tablea where field_bool is null";
            command.Parameters.Clear();

            object result = command.ExecuteScalar(); // The missed cast is needed as Server7.2 returns Int32 and Server7.3+ returns Int64

            command.CommandText = "delete from tablea where field_serial = (select max(field_serial) from tablea);";
            command.ExecuteNonQuery();

            Assert.AreEqual(5, result);

        }

        [Test]
        public void InsertAnsiString()
        {
            try
            {
                _conn.Open();

                EDBCommand command = new EDBCommand("INSERT INTO tablea(field_text) values (:a)", _conn);

                command.Parameters.Add(new EDBParameter("a", DbType.AnsiString));

                command.Parameters[0].Value = "TesteAnsiString";

                Int32 rowsAdded = command.ExecuteNonQuery();

                Assert.AreEqual(1, rowsAdded);

                command.CommandText = string.Format("select count(*) from tablea where field_text = '{0}'", command.Parameters[0].Value);
                command.Parameters.Clear();

                object result = command.ExecuteScalar(); // The missed cast is needed as Server7.2 returns Int32 and Server7.3+ returns Int64

                command.CommandText = "delete from tablea where field_serial = (select max(field_serial) from tablea);";
                command.ExecuteNonQuery();

                Assert.AreEqual(1, result);
            }
            catch (EDBException ex)
            {
                Console.Error.WriteLine(ex.StackTrace);
            }

        }



    }
#pragma warning restore CS8602
}
