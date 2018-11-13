// CommandTests.cs created with MonoDevelop
// User: fxjr at 11:40 PM 4/9/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

// created on 30/11/2002 at 22:35
//
// Author:
//     Francisco Figueiredo Jr. <fxjrlists@yahoo.com>
//
//    Copyright (C) 2002 The Npgsql Development Team
//    npgsql-general@gborg.postgresql.org
//    http://gborg.postgresql.org/project/npgsql/projdisplay.php
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

using System;
using EnterpriseDB.EDBClient;
using NUnit.Framework;
using NUnit.Core;
using System.Data;
using System.Globalization;
using System.Net;
using EDBTypes;
using System.Resources;

namespace NpgsqlTests
{

    public enum EnumTest : short
    {
        Value1 = 0,
        Value2 = 1
    };

    [TestFixture]
    public class CommandTests : BaseClassTests
    {
        protected override EDBConnection TheConnection {
            get { return _conn;}
        }
        protected override EDBTransaction TheTransaction {
            get { return _t; }
            set { _t = value; }
        }
        protected virtual string TheConnectionString {
            get { return _connString; }
        }
        [Test]
        public void ParametersGetName()
        {
            EDBCommand command = new EDBCommand();

            // Add parameters.
            command.Parameters.Add(new EDBParameter(":Parameter1", DbType.Boolean));
            command.Parameters.Add(new EDBParameter(":Parameter2", DbType.Int32));
            command.Parameters.Add(new EDBParameter(":Parameter3", DbType.DateTime));
            command.Parameters.Add(new EDBParameter("Parameter4", DbType.DateTime));

            IDbDataParameter idbPrmtr = command.Parameters["Parameter1"];
            Assert.IsNotNull(idbPrmtr);
            command.Parameters[0].Value = 1;

            // Get by indexers.

            Assert.AreEqual(":Parameter1", command.Parameters[":Parameter1"].ParameterName);
            Assert.AreEqual(":Parameter2", command.Parameters[":Parameter2"].ParameterName);
            Assert.AreEqual(":Parameter3", command.Parameters[":Parameter3"].ParameterName);
            //Assert.AreEqual(":Parameter4", command.Parameters["Parameter4"].ParameterName); //Should this work?

            Assert.AreEqual(":Parameter1", command.Parameters[0].ParameterName);
            Assert.AreEqual(":Parameter2", command.Parameters[1].ParameterName);
            Assert.AreEqual(":Parameter3", command.Parameters[2].ParameterName);
            Assert.AreEqual("Parameter4", command.Parameters[3].ParameterName);
        }
        
        [Test]
        public void ParameterNameWithSpace()
        {
            EDBCommand command = new EDBCommand();

            // Add parameters.
            command.Parameters.Add(new EDBParameter(":Parameter1 ", DbType.Boolean));
            
            Assert.AreEqual(":Parameter1", command.Parameters[0].ParameterName);
            
        }

        [Test]
        public void EmptyQuery()
        {
            
            EDBCommand command = new EDBCommand(";", TheConnection);
            command.ExecuteNonQuery();
        }
        
        
        [Test]
        public void NoNameParameterAdd()
        {
            EDBCommand command = new EDBCommand();

            command.Parameters.Add(new EDBParameter());
            command.Parameters.Add(new EDBParameter());
            
            
            Assert.AreEqual(":Parameter1", command.Parameters[0].ParameterName);
            Assert.AreEqual(":Parameter2", command.Parameters[1].ParameterName);
        }
        

        [Test]
        public void FunctionCallFromSelect()
        {
            EDBCommand command = new EDBCommand("select * from funcB()", TheConnection);

            EDBDataReader reader = command.ExecuteReader();

            Assert.IsNotNull(reader);
            reader.Close();
            //reader.FieldCount
        }

        [Test]
        public void ExecuteScalar()
        {
            EDBCommand command = new EDBCommand("select count(*) from tablea", TheConnection);

            Object result = command.ExecuteScalar();

            Assert.AreEqual(6, result);
            //reader.FieldCount
        }
        
        [Test]
        public void TransactionSetOk()
        {
            EDBCommand command = new EDBCommand("select count(*) from tablea", TheConnection);
            
            command.Transaction = _t;
            
            Object result = command.ExecuteScalar();
            
            Assert.AreEqual(6, result);
        }
        
        
        [Test]
        public void InsertStringWithBackslashes()
        {
            EDBCommand command = new EDBCommand("insert into tablea(field_text) values (:p0)", TheConnection);
            
            command.Parameters.Add(new EDBParameter("p0", EDBDbType.Text));
            
            command.Parameters["p0"].Value = @"\test";

            Object result = command.ExecuteNonQuery();

            Assert.AreEqual(1, result);
            
            
            EDBCommand command2 = new EDBCommand("select field_text from tablea where field_serial = (select max(field_serial) from tablea)", TheConnection);
            

            result = command2.ExecuteScalar();
            
            Assert.AreEqual(@"\test", result);
            
            
            
            //reader.FieldCount
        }
               
        
        [Test]
        public void UseStringParameterWithNoNpgsqlDbType()
        {
            EDBCommand command = new EDBCommand("insert into tablea(field_text) values (:p0)", TheConnection);
            
            command.Parameters.Add(new EDBParameter("p0", "test"));
            
            
            Assert.AreEqual(command.Parameters[0].EDBDbType, EDBDbType.Text);
            Assert.AreEqual(command.Parameters[0].DbType, DbType.String);
            
            Object result = command.ExecuteNonQuery();

            Assert.AreEqual(1, result);
            
            
            EDBCommand command2 = new EDBCommand("select field_text from tablea where field_serial = (select max(field_serial) from tablea)", TheConnection);
            

            result = command2.ExecuteScalar();
            
            
            
            Assert.AreEqual("test", result);
            
            
            
            //reader.FieldCount
        }
        
        
        [Test]
        public void UseIntegerParameterWithNoNpgsqlDbType()
        {
            EDBCommand command = new EDBCommand("insert into tablea(field_int4) values (:p0)", TheConnection);
            
            command.Parameters.Add(new EDBParameter("p0", 5));
            
            Assert.AreEqual(command.Parameters[0].EDBDbType, EDBDbType.Integer);
            Assert.AreEqual(command.Parameters[0].DbType, DbType.Int32);
            
            
            Object result = command.ExecuteNonQuery();

            Assert.AreEqual(1, result);
            
            
            EDBCommand command2 = new EDBCommand("select field_int4 from tablea where field_serial = (select max(field_serial) from tablea)", TheConnection);
            

            result = command2.ExecuteScalar();
            
            
            Assert.AreEqual(5, result);
            
            
            //reader.FieldCount
        }
        
        
        //[Test]
        public void UseSmallintParameterWithNoNpgsqlDbType()
        {
            EDBCommand command = new EDBCommand("insert into tablea(field_int4) values (:p0)", TheConnection);
            
            command.Parameters.Add(new EDBParameter("p0", (Int16)5));
            
            Assert.AreEqual(command.Parameters[0].EDBDbType, EDBDbType.Smallint);
            Assert.AreEqual(command.Parameters[0].DbType, DbType.Int16);
            
            
            Object result = command.ExecuteNonQuery();

            Assert.AreEqual(1, result);
            
            
            EDBCommand command2 = new EDBCommand("select field_int4 from tablea where field_serial = (select max(field_serial) from tablea)", TheConnection);

            result = command2.ExecuteScalar();
            
            
            Assert.AreEqual(5, result);
            
            
            //reader.FieldCount
        }
        
        
        [Test]
        public void FunctionCallReturnSingleValue()
        {
            EDBCommand command = new EDBCommand("funcC();", TheConnection);
            command.CommandType = CommandType.StoredProcedure;

            Object result = command.ExecuteScalar();

            Assert.AreEqual(6, result);
            //reader.FieldCount
        }
        
        
        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void RollbackWithNoTransaction()
        {
            
            TheTransaction.Rollback();
            TheTransaction.Rollback();
        }


        [Test]
        public void FunctionCallReturnSingleValueWithPrepare()
        {
            EDBCommand command = new EDBCommand("funcC()", TheConnection);
            command.CommandType = CommandType.StoredProcedure;

            command.Prepare();
            Object result = command.ExecuteScalar();

            Assert.AreEqual(6, result);
            //reader.FieldCount
        }

        [Test]
        public void FunctionCallWithParametersReturnSingleValue()
        {
            EDBCommand command = new EDBCommand("funcC(:a)", TheConnection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(new EDBParameter("a", DbType.Int32));

            command.Parameters[0].Value = 4;

            Int64 result = (Int64) command.ExecuteScalar();

            Assert.AreEqual(1, result);
        }

        [Test]
        public void FunctionCallWithParametersReturnSingleValueNpgsqlDbType()
        {
            EDBCommand command = new EDBCommand("funcC(:a)", TheConnection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(new EDBParameter("a", EDBDbType.Integer));

            command.Parameters[0].Value = 4;

            Int64 result = (Int64) command.ExecuteScalar();

            Assert.AreEqual(1, result);
        }


        [Test]
        public void FunctionCallWithParametersPrepareReturnSingleValue()
        {
            EDBCommand command = new EDBCommand("funcC(:a)", TheConnection);
            command.CommandType = CommandType.StoredProcedure;


            command.Parameters.Add(new EDBParameter("a", DbType.Int32));

            Assert.AreEqual(1, command.Parameters.Count);
            command.Prepare();


            command.Parameters[0].Value = 4;

            Int64 result = (Int64) command.ExecuteScalar();

            Assert.AreEqual(1, result);
        }

        [Test]
        public void FunctionCallWithParametersPrepareReturnSingleValueNpgsqlDbType()
        {
            EDBCommand command = new EDBCommand("funcC(:a)", TheConnection);
            command.CommandType = CommandType.StoredProcedure;


            command.Parameters.Add(new EDBParameter("a", EDBDbType.Integer));

            Assert.AreEqual(1, command.Parameters.Count);
            command.Prepare();


            command.Parameters[0].Value = 4;

            Int64 result = (Int64) command.ExecuteScalar();

            Assert.AreEqual(1, result);
        }


        [Test]
        public void FunctionCallWithParametersPrepareReturnSingleValueNpgsqlDbType2()
        {
            EDBCommand command = new EDBCommand("funcC(@a)", TheConnection);
            command.CommandType = CommandType.StoredProcedure;


            command.Parameters.Add(new EDBParameter("a", EDBDbType.Integer));

            Assert.AreEqual(1, command.Parameters.Count);
            //command.Prepare();


            command.Parameters[0].Value = 4;

            Int64 result = (Int64) command.ExecuteScalar();

            Assert.AreEqual(1, result);
        }


        [Test]
        public void FunctionCallReturnResultSet()
        {
            EDBCommand command = new EDBCommand("funcB()", TheConnection);
            command.CommandType = CommandType.StoredProcedure;

            EDBDataReader dr = command.ExecuteReader();
            Assert.IsNotNull(dr);
            dr.Close();
        }


        [Test]
        public void CursorStatement()
        {
            Int32 i = 0;

            
            EDBCommand command = new EDBCommand("declare te cursor for select * from tablea;", TheConnection);

            command.ExecuteNonQuery();

            command.CommandText = "fetch forward 3 in te;";

            EDBDataReader dr = command.ExecuteReader();


            while (dr.Read())
            {
                i++;
            }

            Assert.AreEqual(3, i);


            i = 0;

            command.CommandText = "fetch backward 1 in te;";

            EDBDataReader dr2 = command.ExecuteReader();

            while (dr2.Read())
            {
                i++;
            }

            Assert.AreEqual(1, i);

            command.CommandText = "close te;";

            command.ExecuteNonQuery();
        }

        [Test]
        public void PreparedStatementNoParameters()
        {
            EDBCommand command = new EDBCommand("select * from tablea;", TheConnection);

            command.Prepare();
            
            EDBDataReader dr = command.ExecuteReader();
            Assert.IsNotNull(dr);
            
            dr.Close();
        }
        
        
        [Test]
        public void PreparedStatementInsert()
        {
            EDBCommand command = new EDBCommand("insert into tablea(field_text) values (:p0);", TheConnection);
            command.Parameters.Add(new EDBParameter("p0", EDBDbType.Text));
            command.Parameters["p0"].Value = "test";
            
            command.Prepare();
            
            EDBDataReader dr = command.ExecuteReader();
            Assert.IsNotNull(dr);
        }
        
        [Test]
        public void RTFStatementInsert()
        {
            EDBCommand command = new EDBCommand("insert into tablea(field_text) values (:p0);", TheConnection);
            command.Parameters.Add(new EDBParameter("p0", EDBDbType.Text));
            command.Parameters["p0"].Value = @"{\rtf1\ansi\ansicpg1252\uc1 \deff0\deflang1033\deflangfe1033{";
                       
            
            EDBDataReader dr = command.ExecuteReader();
            Assert.IsNotNull(dr);
            
            
            String result = (String)new EDBCommand("select field_text from tablea where field_serial = (select max(field_serial) from tablea);", TheConnection).ExecuteScalar();
            
            Assert.AreEqual(@"{\rtf1\ansi\ansicpg1252\uc1 \deff0\deflang1033\deflangfe1033{", result);
        }
        
        
        
        [Test]
        public void PreparedStatementInsertNullValue()
        {
            EDBCommand command = new EDBCommand("insert into tablea(field_int4) values (:p0);", TheConnection);
            command.Parameters.Add(new EDBParameter("p0", EDBDbType.Integer));
            command.Parameters["p0"].Value = DBNull.Value;

            command.Prepare();

            EDBDataReader dr = command.ExecuteReader();
            Assert.IsNotNull(dr);
        }
        

        [Test]
        public void PreparedStatementWithParameters()
        {
            EDBCommand command = new EDBCommand("select * from tablea where field_int4 = :a and field_int8 = :b;", TheConnection);

            command.Parameters.Add(new EDBParameter("a", DbType.Int32));
            command.Parameters.Add(new EDBParameter("b", DbType.Int64));

            Assert.AreEqual(2, command.Parameters.Count);

            Assert.AreEqual(DbType.Int32, command.Parameters[0].DbType);

            command.Prepare();

            command.Parameters[0].Value = 3;
            command.Parameters[1].Value = 5;

            EDBDataReader dr = command.ExecuteReader();
            Assert.IsNotNull(dr);
            
            dr.Close();
        }

        [Test]
        public void PreparedStatementWithParametersNpgsqlDbType()
        {
            EDBCommand command = new EDBCommand("select * from tablea where field_int4 = :a and field_int8 = :b;", TheConnection);

            command.Parameters.Add(new EDBParameter("a", EDBDbType.Integer));
            command.Parameters.Add(new EDBParameter("b", EDBDbType.Bigint));

            Assert.AreEqual(2, command.Parameters.Count);

            Assert.AreEqual(DbType.Int32, command.Parameters[0].DbType);

            command.Prepare();

            command.Parameters[0].Value = 3;
            command.Parameters[1].Value = 5;

            EDBDataReader dr = command.ExecuteReader();
            Assert.IsNotNull(dr);
            dr.Close();
        }
        
        [Test]
        public void FunctionCallWithImplicitParameters()
        {
            EDBCommand command = new EDBCommand("funcC", TheConnection);
            command.CommandType = CommandType.StoredProcedure;


            EDBParameter p = new EDBParameter("@a", EDBDbType.Integer);
            p.Direction = ParameterDirection.InputOutput;
            p.Value = 4;
            
            command.Parameters.Add(p);
            

            Int64 result = (Int64) command.ExecuteScalar();

            Assert.AreEqual(1, result);
        }
        
        
        [Test]
        public void PreparedFunctionCallWithImplicitParameters()
        {
            EDBCommand command = new EDBCommand("funcC", TheConnection);
            command.CommandType = CommandType.StoredProcedure;


            EDBParameter p = new EDBParameter("a", EDBDbType.Integer);
            p.Direction = ParameterDirection.InputOutput;
            p.Value = 4;
            
            command.Parameters.Add(p);
            
            command.Prepare();

            Int64 result = (Int64) command.ExecuteScalar();

            Assert.AreEqual(1, result);
        }
        
        
        [Test]
        public void FunctionCallWithImplicitParametersWithNoParameters()
        {
            EDBCommand command = new EDBCommand("funcC", TheConnection);
            command.CommandType = CommandType.StoredProcedure;

            Object result = command.ExecuteScalar();

            Assert.AreEqual(6, result);
            //reader.FieldCount

        }
        
        [Test]
        public void FunctionCallOutputParameter()
        {
            EDBCommand command = new EDBCommand("funcC()", TheConnection);
            command.CommandType = CommandType.StoredProcedure;
            
            EDBParameter p = new EDBParameter("a", DbType.Int32);
            p.Direction = ParameterDirection.Output;
            p.Value = -1;
            
            command.Parameters.Add(p);
            
                        
            command.ExecuteNonQuery();
            
            Assert.AreEqual(6, command.Parameters["a"].Value);
        }
        
        [Test]
        public void FunctionCallOutputParameter2()
        {
            EDBCommand command = new EDBCommand("funcC", TheConnection);
            command.CommandType = CommandType.StoredProcedure;
            
            EDBParameter p = new EDBParameter("@a", DbType.Int32);
            p.Direction = ParameterDirection.Output;
            p.Value = -1;
            
            command.Parameters.Add(p);
            
                        
            command.ExecuteNonQuery();
            
            Assert.AreEqual(6, command.Parameters["@a"].Value);
        }
        
        [Test]
        public void OutputParameterWithoutName()
        {
            EDBCommand command = new EDBCommand("funcC", TheConnection);
            command.CommandType = CommandType.StoredProcedure;
            
            EDBParameter p = command.CreateParameter();
            p.Direction = ParameterDirection.Output;
            p.Value = -1;
            
            command.Parameters.Add(p);
            
                        
            command.ExecuteNonQuery();
            
            Assert.AreEqual(6, command.Parameters[0].Value);
        }
        
        [Test]
        public void FunctionReturnVoid()
        {
            EDBCommand command = new EDBCommand("testreturnvoid()", TheConnection);
            command.CommandType = CommandType.StoredProcedure;
            command.ExecuteNonQuery();
        }
        
        [Test]
        public void StatementOutputParameters()
        {
            EDBCommand command = new EDBCommand("select 4, 5;", TheConnection);
                        
            EDBParameter p = new EDBParameter("a", DbType.Int32);
            p.Direction = ParameterDirection.Output;
            p.Value = -1;
            
            command.Parameters.Add(p);
            
            p = new EDBParameter("b", DbType.Int32);
            p.Direction = ParameterDirection.Output;
            p.Value = -1;
            
            command.Parameters.Add(p);
            
            
            p = new EDBParameter("c", DbType.Int32);
            p.Direction = ParameterDirection.Output;
            p.Value = -1;
            
            command.Parameters.Add(p);
            
                        
            command.ExecuteNonQuery();
            
            Assert.AreEqual(4, command.Parameters["a"].Value);
            Assert.AreEqual(5, command.Parameters["b"].Value);
            Assert.AreEqual(-1, command.Parameters["c"].Value);
        }
        
        [Test]
        public void FunctionCallInputOutputParameter()
        {
            EDBCommand command = new EDBCommand("funcC(:a)", TheConnection);
            command.CommandType = CommandType.StoredProcedure;


            EDBParameter p = new EDBParameter("a", EDBDbType.Integer);
            p.Direction = ParameterDirection.InputOutput;
            p.Value = 4;
            
            command.Parameters.Add(p);
            

            Int64 result = (Int64) command.ExecuteScalar();

            Assert.AreEqual(1, result);
        }
        
        
        [Test]
        public void StatementMappedOutputParameters()
        {
            EDBCommand command = new EDBCommand("select 3, 4 as param1, 5 as param2, 6;", TheConnection);
                        
            EDBParameter p = new EDBParameter("param2", EDBDbType.Integer);
            p.Direction = ParameterDirection.Output;
            p.Value = -1;
            
            command.Parameters.Add(p);
            
            p = new EDBParameter("param1", EDBDbType.Integer);
            p.Direction = ParameterDirection.Output;
            p.Value = -1;
            
            command.Parameters.Add(p);
            
            p = new EDBParameter("p", EDBDbType.Integer);
            p.Direction = ParameterDirection.Output;
            p.Value = -1;
            
            command.Parameters.Add(p);
            
            
            command.ExecuteNonQuery();
            
            Assert.AreEqual(4, command.Parameters["param1"].Value);
            Assert.AreEqual(5, command.Parameters["param2"].Value);
            //Assert.AreEqual(-1, command.Parameters["p"].Value); //Which is better, not filling this or filling this with an unmapped value?
        }


        [Test]
        public void ListenNotifySupport()
        {
            // Notify messages are only sent from server after a transaction is finished.
            // So, finish now the implicit transaction.
            
            TheTransaction.Rollback();
            
            Assert.IsFalse(RecievedNotification);//Test we start correctly.

            EDBCommand command = new EDBCommand("listen notifytest;", TheConnection);
            command.ExecuteNonQuery();

            TheConnection.Notification += new NotificationEventHandler(NotificationSupportHelper);


            command = new EDBCommand("notify notifytest;", TheConnection);
            command.ExecuteNonQuery();

            Assert.IsTrue(RecievedNotification);
            
        }

        public bool RecievedNotification = false;
        private void NotificationSupportHelper(Object sender, EDBNotificationEventArgs args)
        {
            RecievedNotification = true;
        }

        
            [Test]
        public void ByteSupport()
        {
            EDBCommand command = new EDBCommand("insert into tableb(field_int2) values (:a)", TheConnection);

            command.Parameters.Add(new EDBParameter("a", DbType.Byte));

            command.Parameters[0].Value = 2;

            Int32 rowsAdded = command.ExecuteNonQuery();

            Assert.AreEqual(1, rowsAdded);

            command.Parameters.Clear();
        }
        
        
            [Test]
        public void ByteaSupport()
        {
            EDBCommand command = new EDBCommand("select field_bytea from tablef where field_serial = 1", TheConnection);


            Byte[] result = (Byte[]) command.ExecuteScalar();
            

            Assert.AreEqual(2, result.Length);
        }
        
        [Test]
        public void ByteaInsertSupport()
        {
            Byte[] toStore = { 1 };

                  EDBCommand cmd = new EDBCommand("insert into tablef(field_bytea) values (:val)", TheConnection);
              cmd.Parameters.Add(new EDBParameter("val", DbType.Binary));
                  cmd.Parameters[0].Value = toStore;
                  cmd.ExecuteNonQuery();

                  cmd = new EDBCommand("select field_bytea from tablef where field_serial = (select max(field_serial) from tablef)", TheConnection);
            
                  Byte[] result = (Byte[])cmd.ExecuteScalar();
            
            Assert.AreEqual(1, result.Length);

        }
        
        [Test]
        public void ByteaInsertWithPrepareSupport()
        {
            


            Byte[] toStore = { 1 };

            EDBCommand cmd = new EDBCommand("insert into tablef(field_bytea) values (:val)", TheConnection);
            cmd.Parameters.Add(new EDBParameter("val", DbType.Binary));
            cmd.Parameters[0].Value = toStore;
            cmd.Prepare();
            cmd.ExecuteNonQuery();

            cmd = new EDBCommand("select field_bytea from tablef where field_serial = (select max(field_serial) from tablef)", TheConnection);
            
            cmd.Prepare();
            Byte[] result = (Byte[])cmd.ExecuteScalar();
            
            
            Assert.AreEqual(toStore, result);
        }
        
        
        
        [Test]
        public void ByteaParameterSupport()
        {
            EDBCommand command = new EDBCommand("select field_bytea from tablef where field_bytea = :bytesData", TheConnection);
            
            Byte[] bytes = new Byte[]{45,44};
            
            command.Parameters.Add(":bytesData", EDBTypes.EDBDbType.Bytea);
                  command.Parameters[":bytesData"].Value = bytes;


            Object result = command.ExecuteNonQuery();
            

            Assert.AreEqual(-1, result);
        }
        
        [Test]
        public void ByteaParameterWithPrepareSupport()
        {
            EDBCommand command = new EDBCommand("select field_bytea from tablef where field_bytea = :bytesData", TheConnection);
            
            Byte[] bytes = new Byte[]{45,44};
            
            command.Parameters.Add(":bytesData", EDBTypes.EDBDbType.Bytea);
                  command.Parameters[":bytesData"].Value = bytes;


            command.Prepare();
            Object result = command.ExecuteNonQuery();
            

            Assert.AreEqual(-1, result);
        }
        
        
            [Test]
        public void EnumSupport()
        {
            EDBCommand command = new EDBCommand("insert into tableb(field_int2) values (:a)", TheConnection);

            command.Parameters.Add(new EDBParameter("a", EDBDbType.Smallint));

            command.Parameters[0].Value = EnumTest.Value1;
            

            Int32 rowsAdded = command.ExecuteNonQuery();

            Assert.AreEqual(1, rowsAdded);
        }

        [Test]
        public void DateTimeSupport()
        {
            EDBCommand command = new EDBCommand("select field_timestamp from tableb where field_serial = 2;", TheConnection);

            DateTime d = (DateTime)command.ExecuteScalar();


            Assert.AreEqual("2002-02-02 09:00:23Z", d.ToString("u"));

            DateTimeFormatInfo culture = new DateTimeFormatInfo();
            culture.TimeSeparator = ":";
            DateTime dt = System.DateTime.Parse("2004-06-04 09:48:00", culture);

            command.CommandText = "insert into tableb(field_timestamp) values (:a);";
            command.Parameters.Add(new EDBParameter("a", DbType.DateTime));
            command.Parameters[0].Value = dt;

            command.ExecuteScalar();
        }


        [Test]
        public void DateTimeSupportNpgsqlDbType()
        {
            EDBCommand command = new EDBCommand("select field_timestamp from tableb where field_serial = 2;", TheConnection);

            DateTime d = (DateTime)command.ExecuteScalar();


            Assert.AreEqual("2002-02-02 09:00:23Z", d.ToString("u"));

            DateTimeFormatInfo culture = new DateTimeFormatInfo();
            culture.TimeSeparator = ":";
            DateTime dt = System.DateTime.Parse("2004-06-04 09:48:00", culture);

            command.CommandText = "insert into tableb(field_timestamp) values (:a);";
            command.Parameters.Add(new EDBParameter("a", EDBDbType.Timestamp));
            command.Parameters[0].Value = dt;

            command.ExecuteScalar();
        }

        [Test]
        public void DateSupport()
        {
            EDBCommand command = new EDBCommand("select field_date from tablec where field_serial = 1;", TheConnection);

            DateTime d = (DateTime)command.ExecuteScalar();


            Assert.AreEqual("2002-03-04", d.ToString("yyyy-MM-dd"));
        }

        [Test]
        public void TimeSupport()
        {
            EDBCommand command = new EDBCommand("select field_time from tablec where field_serial = 2;", TheConnection);

            DateTime d = (DateTime)command.ExecuteScalar();


            Assert.AreEqual("10:03:45.345", d.ToString("HH:mm:ss.fff"));
        }
        
        [Test]
        public void DateTimeSupportTimezone()
        {
            EDBCommand command = new EDBCommand("set time zone 5;select field_timestamp_with_timezone from tableg where field_serial = 1;", TheConnection);
            
            DateTime d = (DateTime)command.ExecuteScalar();
            
            
            Assert.AreEqual("2002-02-02 09:00:23Z", d.ToString("u"));
        }

        [Test]
        public void DateTimeSupportTimezone2()
        {
            //Changed the comparison. Did thisassume too much about ToString()?
            EDBCommand command = new EDBCommand("set time zone 5; select field_timestamp_with_timezone from tableg where field_serial = 1;", TheConnection);
            
            String s = command.ExecuteScalar().ToString();
           
            Assert.AreEqual(new DateTime(2002,02,02,09,00,23).ToString() , s);
        }


        [Test]
        public void NumericSupport()
        {
            EDBCommand command = new EDBCommand("insert into tableb(field_numeric) values (:a)", TheConnection);
            command.Parameters.Add(new EDBParameter("a", DbType.Decimal));

            command.Parameters[0].Value = 7.4M;

            Int32 rowsAdded = command.ExecuteNonQuery();

            Assert.AreEqual(1, rowsAdded);

            command.CommandText = "select * from tableb where field_numeric = :a";


            EDBDataReader dr = command.ExecuteReader();
            dr.Read();

            Decimal result = dr.GetDecimal(3);


            Assert.AreEqual(7.4000000M, result);
            dr.Close();
        }

        [Test]
        public void NumericSupportNpgsqlDbType()
        {
            EDBCommand command = new EDBCommand("insert into tableb(field_numeric) values (:a)", TheConnection);
            command.Parameters.Add(new EDBParameter("a", EDBDbType.Numeric));

            command.Parameters[0].Value = 7.4M;

            Int32 rowsAdded = command.ExecuteNonQuery();

            Assert.AreEqual(1, rowsAdded);

            command.CommandText = "select * from tableb where field_numeric = :a";


            EDBDataReader dr = command.ExecuteReader();
            dr.Read();

            Decimal result = dr.GetDecimal(3);


            Assert.AreEqual(7.4000000M, result);
            
            dr.Close();
        }


        [Test]
        public void InsertSingleValue()
        {
            EDBCommand command = new EDBCommand("insert into tabled(field_float4) values (:a)", TheConnection);
            command.Parameters.Add(new EDBParameter(":a", DbType.Single));

            command.Parameters[0].Value = 7.4F;

            Int32 rowsAdded = command.ExecuteNonQuery();

            Assert.AreEqual(1, rowsAdded);

            command.CommandText = "select * from tabled where field_float4 = :a";


            EDBDataReader dr = command.ExecuteReader();
            dr.Read();

            Single result = dr.GetFloat(1);


            Assert.AreEqual(7.4F, result);
            
            dr.Close();
        }


        [Test]
        public void InsertSingleValueNpgsqlDbType()
        {
            EDBCommand command = new EDBCommand("insert into tabled(field_float4) values (:a)", TheConnection);
            command.Parameters.Add(new EDBParameter(":a", EDBDbType.Real));

            command.Parameters[0].Value = 7.4F;

            Int32 rowsAdded = command.ExecuteNonQuery();

            Assert.AreEqual(1, rowsAdded);

            command.CommandText = "select * from tabled where field_float4 = :a";


            EDBDataReader dr = command.ExecuteReader();
            dr.Read();

            Single result = dr.GetFloat(1);


            Assert.AreEqual(7.4F, result);
            
            dr.Close();
        }
        
        
        [Test]
        public void DoubleValueSupportWithExtendedQuery()
        {
            EDBCommand command = new EDBCommand("select count(*) from tabled where field_float8 = :a", TheConnection);
            command.Parameters.Add(new EDBParameter(":a", EDBDbType.Double));

            command.Parameters[0].Value = 0.123456789012345D;
            
            command.Prepare();

            Object rows = command.ExecuteScalar();

            
            Assert.AreEqual(1, rows);
        }

        [Test]
        public void InsertDoubleValue()
        {
            EDBCommand command = new EDBCommand("insert into tabled(field_float8) values (:a)", TheConnection);
            command.Parameters.Add(new EDBParameter(":a", DbType.Double));

            command.Parameters[0].Value = 7.4D;

            Int32 rowsAdded = command.ExecuteNonQuery();

            command.CommandText = "select * from tabled where field_float8 = :a";


            EDBDataReader dr = command.ExecuteReader();
            dr.Read();

            Double result = dr.GetDouble(2);


            Assert.AreEqual(1, rowsAdded);
            Assert.AreEqual(7.4D, result);
            
            dr.Close();
        }


        [Test]
        public void InsertDoubleValueNpgsqlDbType()
        {
            EDBCommand command = new EDBCommand("insert into tabled(field_float8) values (:a)", TheConnection);
            command.Parameters.Add(new EDBParameter(":a", EDBDbType.Double));

            command.Parameters[0].Value = 7.4D;

            Int32 rowsAdded = command.ExecuteNonQuery();

            command.CommandText = "select * from tabled where field_float8 = :a";


            EDBDataReader dr = command.ExecuteReader();
            dr.Read();

            Double result = dr.GetDouble(2);


            Assert.AreEqual(1, rowsAdded);
            Assert.AreEqual(7.4D, result);
            
            dr.Close();
        }


        [Test]
        public void NegativeNumericSupport()
        {
            EDBCommand command = new EDBCommand("select * from tableb where field_serial = 4", TheConnection);


            EDBDataReader dr = command.ExecuteReader();
            dr.Read();

            Decimal result = dr.GetDecimal(3);

            Assert.AreEqual(-4.3000000M, result);
            
            dr.Close();
        }


        [Test]
        public void PrecisionScaleNumericSupport()
        {
            EDBCommand command = new EDBCommand("select * from tableb where field_serial = 4", TheConnection);


            EDBDataReader dr = command.ExecuteReader();
            dr.Read();

            Decimal result = dr.GetDecimal(3);

            Assert.AreEqual(-4.3000000M, result);
            //Assert.AreEqual(11, result.Precision);
            //Assert.AreEqual(7, result.Scale);
            
            dr.Close();
        }

        [Test]
        public void InsertNullString()
        {
            EDBCommand command = new EDBCommand("insert into tablea(field_text) values (:a)", TheConnection);

            command.Parameters.Add(new EDBParameter("a", DbType.String));

            command.Parameters[0].Value = DBNull.Value;

            Int32 rowsAdded = command.ExecuteNonQuery();

            Assert.AreEqual(1, rowsAdded);

            command.CommandText = "select count(*) from tablea where field_text is null";
            command.Parameters.Clear();

            Int64 result = (Int64)command.ExecuteScalar();

            Assert.AreEqual(4, result);
        }

        [Test]
        public void InsertNullStringNpgsqlDbType()
        {
            EDBCommand command = new EDBCommand("insert into tablea(field_text) values (:a)", TheConnection);

            command.Parameters.Add(new EDBParameter("a", EDBDbType.Text));

            command.Parameters[0].Value = DBNull.Value;

            Int32 rowsAdded = command.ExecuteNonQuery();

            Assert.AreEqual(1, rowsAdded);

            command.CommandText = "select count(*) from tablea where field_text is null";
            command.Parameters.Clear();

            Int64 result = (Int64)command.ExecuteScalar();

            Assert.AreEqual(4, result);
        }



        [Test]
        public void InsertNullDateTime()
        {
            EDBCommand command = new EDBCommand("insert into tableb(field_timestamp) values (:a)", TheConnection);

            command.Parameters.Add(new EDBParameter("a", DbType.DateTime));

            command.Parameters[0].Value = DBNull.Value;

            Int32 rowsAdded = command.ExecuteNonQuery();

            Assert.AreEqual(1, rowsAdded);

            command.CommandText = "select count(*) from tableb where field_timestamp is null";
            command.Parameters.Clear();

            Object result = command.ExecuteScalar();

            Assert.AreEqual(4, result);
        }


        [Test]
        public void InsertNullDateTimeNpgsqlDbType()
        {
            EDBCommand command = new EDBCommand("insert into tableb(field_timestamp) values (:a)", TheConnection);

            command.Parameters.Add(new EDBParameter("a", EDBDbType.Timestamp));

            command.Parameters[0].Value = DBNull.Value;

            Int32 rowsAdded = command.ExecuteNonQuery();

            Assert.AreEqual(1, rowsAdded);

            command.CommandText = "select count(*) from tableb where field_timestamp is null";
            command.Parameters.Clear();

            Object result = command.ExecuteScalar();

            Assert.AreEqual(4, result);
        }



        [Test]
        public void InsertNullInt16()
        {
            EDBCommand command = new EDBCommand("insert into tableb(field_int2) values (:a)", TheConnection);

            command.Parameters.Add(new EDBParameter("a", DbType.Int16));

            command.Parameters[0].Value = DBNull.Value;

            Int32 rowsAdded = command.ExecuteNonQuery();

            Assert.AreEqual(1, rowsAdded);

            command.CommandText = "select count(*) from tableb where field_int2 is null";
            command.Parameters.Clear();

            Object result = command.ExecuteScalar(); // The missed cast is needed as Server7.2 returns Int32 and Server7.3+ returns Int64

            Assert.AreEqual(4, result);
        }


        [Test]
        public void InsertNullInt16NpgsqlDbType()
        {
            EDBCommand command = new EDBCommand("insert into tableb(field_int2) values (:a)", TheConnection);

            command.Parameters.Add(new EDBParameter("a", EDBDbType.Smallint));

            command.Parameters[0].Value = DBNull.Value;

            Int32 rowsAdded = command.ExecuteNonQuery();

            Assert.AreEqual(1, rowsAdded);

            command.CommandText = "select count(*) from tableb where field_int2 is null";
            command.Parameters.Clear();

            Object result = command.ExecuteScalar(); // The missed cast is needed as Server7.2 returns Int32 and Server7.3+ returns Int64

            Assert.AreEqual(4, result);
        }


        [Test]
        public void InsertNullInt32()
        {
            EDBCommand command = new EDBCommand("insert into tablea(field_int4) values (:a)", TheConnection);

            command.Parameters.Add(new EDBParameter("a", DbType.Int32));

            command.Parameters[0].Value = DBNull.Value;

            Int32 rowsAdded = command.ExecuteNonQuery();

            Assert.AreEqual(1, rowsAdded);

            command.CommandText = "select count(*) from tablea where field_int4 is null";
            command.Parameters.Clear();

            Object result = command.ExecuteScalar(); // The missed cast is needed as Server7.2 returns Int32 and Server7.3+ returns Int64

            Assert.AreEqual(6, result);
        }


        [Test]
        public void InsertNullNumeric()
        {
            EDBCommand command = new EDBCommand("insert into tableb(field_numeric) values (:a)", TheConnection);

            command.Parameters.Add(new EDBParameter("a", DbType.Decimal));

            command.Parameters[0].Value = DBNull.Value;

            Int32 rowsAdded = command.ExecuteNonQuery();

            Assert.AreEqual(1, rowsAdded);

            command.CommandText = "select count(*) from tableb where field_numeric is null";
            command.Parameters.Clear();

            Object result = command.ExecuteScalar(); // The missed cast is needed as Server7.2 returns Int32 and Server7.3+ returns Int64

            Assert.AreEqual(3, result);
        }

        [Test]
        public void InsertNullBoolean()
        {
            EDBCommand command = new EDBCommand("insert into tablea(field_bool) values (:a)", TheConnection);

            command.Parameters.Add(new EDBParameter("a", DbType.Boolean));

            command.Parameters[0].Value = DBNull.Value;

            Int32 rowsAdded = command.ExecuteNonQuery();

            Assert.AreEqual(1, rowsAdded);

            command.CommandText = "select count(*) from tablea where field_bool is null";
            command.Parameters.Clear();

            Object result = command.ExecuteScalar(); // The missed cast is needed as Server7.2 returns Int32 and Server7.3+ returns Int64

            Assert.AreEqual(6, result);

        }

        [Test]
        public void InsertBoolean()
        {
            


            EDBCommand command = new EDBCommand("insert into tablea(field_bool) values (:a)", TheConnection);

            command.Parameters.Add(new EDBParameter("a", DbType.Boolean));

            command.Parameters[0].Value = false;

            Int32 rowsAdded = command.ExecuteNonQuery();

            Assert.AreEqual(1, rowsAdded);

            command.CommandText = "select field_bool from tablea where field_serial = (select max(field_serial) from tablea)";
            command.Parameters.Clear();

            Object result = command.ExecuteScalar(); // The missed cast is needed as Server7.2 returns Int32 and Server7.3+ returns Int64

            Assert.AreEqual(false, result);

        }

        [Test]
        public void AnsiStringSupport()
        {
         
            EDBCommand command = new EDBCommand("insert into tablea(field_text) values (:a)", TheConnection);

            command.Parameters.Add(new EDBParameter("a", DbType.AnsiString));

            command.Parameters[0].Value = "TesteAnsiString";

            Int32 rowsAdded = command.ExecuteNonQuery();

            Assert.AreEqual(1, rowsAdded);

            command.CommandText = String.Format("select count(*) from tablea where field_text = '{0}'", command.Parameters[0].Value);
            command.Parameters.Clear();

            Object result = command.ExecuteScalar(); // The missed cast is needed as Server7.2 returns Int32 and Server7.3+ returns Int64

            Assert.AreEqual(1, result);
        }


        [Test]
        public void MultipleQueriesFirstResultsetEmpty()
        {
            EDBCommand command = new EDBCommand("insert into tablea(field_text) values ('a'); select count(*) from tablea;", TheConnection);

            Object result = command.ExecuteScalar();

            Assert.AreEqual(7, result);
        }

        [Test]
        [ExpectedException(typeof(EDBException))]
        public void ConnectionStringWithInvalidParameterValue()
        {
            EDBConnection conn = new EDBConnection("Server=127.0.0.1;User Id=npgsql_tets;Password=j");

            EDBCommand command = new EDBCommand("select * from tablea", conn);

            command.Connection.Open();
            command.ExecuteReader();
            command.Connection.Close();
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void InvalidConnectionString()
        {
            EDBConnection conn = new EDBConnection("Server=127.0.0.1;User Id=npgsql_tests;Pooling:false");

            EDBCommand command = new EDBCommand("select * from tablea", conn);

            command.Connection.Open();
            command.ExecuteReader();
            command.Connection.Close();
        }


        [Test]
        public void AmbiguousFunctionParameterType()
        {
            EDBConnection conn = new EDBConnection(TheConnectionString);


            EDBCommand command = new EDBCommand("ambiguousParameterType(:a, :b, :c, :d, :e, :f)", conn);
            command.CommandType = CommandType.StoredProcedure;
            EDBParameter p = new EDBParameter("a", DbType.Int16);
            p.Value = 2;
            command.Parameters.Add(p);
            p = new EDBParameter("b", DbType.Int32);
            p.Value = 2;
            command.Parameters.Add(p);
            p = new EDBParameter("c", DbType.Int64);
            p.Value = 2;
            command.Parameters.Add(p);
            p = new EDBParameter("d", DbType.String);
            p.Value = "a";
            command.Parameters.Add(p);
            p = new EDBParameter("e", EDBDbType.Char);
            p.Value = "a";
            command.Parameters.Add(p);
            p = new EDBParameter("f", EDBDbType.Varchar);
            p.Value = "a";
            command.Parameters.Add(p);


            command.Connection.Open();
            command.ExecuteScalar();
            command.Connection.Close();
        }
        
        [Test]
        public void AmbiguousFunctionParameterTypePrepared()
        {
            EDBConnection conn = new EDBConnection(TheConnectionString);


            EDBCommand command = new EDBCommand("ambiguousParameterType(:a, :b, :c, :d, :e, :f)", conn);
            command.CommandType = CommandType.StoredProcedure;
            EDBParameter p = new EDBParameter("a", DbType.Int16);
            p.Value = 2;
            command.Parameters.Add(p);
            p = new EDBParameter("b", DbType.Int32);
            p.Value = 2;
            command.Parameters.Add(p);
            p = new EDBParameter("c", DbType.Int64);
            p.Value = 2;
            command.Parameters.Add(p);
            p = new EDBParameter("d", DbType.String);
            p.Value = "a";
            command.Parameters.Add(p);
            p = new EDBParameter("e", DbType.String);
            p.Value = "a";
            command.Parameters.Add(p);
            p = new EDBParameter("f", DbType.String);
            p.Value = "a";
            command.Parameters.Add(p);


            command.Connection.Open();
            command.Prepare();
            command.ExecuteScalar();
            command.Connection.Close();
        }


        
        // The following two methods don't need checks because what is being tested is the 
        // execution of parameter replacing which happens on ExecuteNonQuery method. So, if these
        // methods don't throw exception, they are ok.
        [Test]
        public void TestParameterReplace()
        {
            String sql = @"select * from tablea where
                            field_serial = :a
                         ";


            EDBCommand command = new EDBCommand(sql, TheConnection);

            command.Parameters.Add(new EDBParameter("a", DbType.Int32));

            command.Parameters[0].Value = 2;

            command.ExecuteNonQuery();
        }
        
        [Test]
        public void TestParameterReplace2()
        {
            String sql = @"select * from tablea where
                         field_serial = :a+1
                         ";


            EDBCommand command = new EDBCommand(sql, TheConnection);

            command.Parameters.Add(new EDBParameter("a", DbType.Int32));

            command.Parameters[0].Value = 1;

            command.ExecuteNonQuery();
        }
        
        [Test]
        public void TestParameterNameInParameterValue()
        {
            String sql = "insert into tablea(field_text, field_int4) values ( :a, :b );" ;

            String aValue = "test :b";

            EDBCommand command = new EDBCommand(sql, TheConnection);

            command.Parameters.Add(new EDBParameter(":a", EDBDbType.Text));

            command.Parameters[":a"].Value = aValue;
            
            command.Parameters.Add(new EDBParameter(":b", EDBDbType.Integer));

            command.Parameters[":b"].Value = 1;

            Int32 rowsAdded = command.ExecuteNonQuery();
            Assert.AreEqual(rowsAdded, 1);
            
            
            EDBCommand command2 = new EDBCommand("select field_text, field_int4 from tablea where field_serial = (select max(field_serial) from tablea)", TheConnection);
            
            EDBDataReader dr = command2.ExecuteReader();
            
            dr.Read();
            
            String a = dr.GetString(0);;
            Int32 b = dr.GetInt32(1);
            
            dr.Close();
            
            
            
            Assert.AreEqual(aValue, a);
            Assert.AreEqual(1, b);
        }

        [Test]
        public void TestBoolParameter1()
        {
            // will throw exception if bool parameter can't be used as boolean expression
            EDBCommand command = new EDBCommand("select case when (foo is null) then false else foo end as bar from (select :a as foo) as x", TheConnection);
            EDBParameter p0 = new EDBParameter(":a", true);
            // with setting pramater type
            p0.DbType = DbType.Boolean;
            command.Parameters.Add(p0);

            command.ExecuteScalar();
        }

        [Test]
        public void TestBoolParameter2()
        {
            // will throw exception if bool parameter can't be used as boolean expression
            EDBCommand command = new EDBCommand("select case when (foo is null) then false else foo end as bar from (select :a as foo) as x", TheConnection);
            EDBParameter p0 = new EDBParameter(":a", true);
            // not setting parameter type
            command.Parameters.Add(p0);

            command.ExecuteScalar();
        }

        [Test]
        public void TestPointSupport()
        {
            EDBCommand command = new EDBCommand("select field_point from tablee where field_serial = 1", TheConnection);

            EDBPoint p = (EDBPoint) command.ExecuteScalar();

            Assert.AreEqual(4, p.X);
            Assert.AreEqual(3, p.Y);
        }


        [Test]
        public void TestBoxSupport()
        {
            EDBCommand command = new EDBCommand("select field_box from tablee where field_serial = 2", TheConnection);

            EDBBox box = (EDBBox) command.ExecuteScalar();

            Assert.AreEqual(5, box.UpperRight.X);
            Assert.AreEqual(4, box.UpperRight.Y);
            Assert.AreEqual(4, box.LowerLeft.X);
            Assert.AreEqual(3, box.LowerLeft.Y);
        }

        [Test]
        public void TestLSegSupport()
        {
            EDBCommand command = new EDBCommand("select field_lseg from tablee where field_serial = 3", TheConnection);

            EDBLSeg lseg = (EDBLSeg) command.ExecuteScalar();

            Assert.AreEqual(4, lseg.Start.X);
            Assert.AreEqual(3, lseg.Start.Y);
            Assert.AreEqual(5, lseg.End.X);
            Assert.AreEqual(4, lseg.End.Y);
        }

        [Test]
        public void TestClosedPathSupport()
        {
            EDBCommand command = new EDBCommand("select field_path from tablee where field_serial = 4", TheConnection);

            EDBPath path = (EDBPath) command.ExecuteScalar();

            Assert.AreEqual(false, path.Open);
            Assert.AreEqual(2, path.Count);
            Assert.AreEqual(4, path[0].X);
            Assert.AreEqual(3, path[0].Y);
            Assert.AreEqual(5, path[1].X);
            Assert.AreEqual(4, path[1].Y);
        }

        [Test]
        public void TestOpenPathSupport()
        {
            EDBCommand command = new EDBCommand("select field_path from tablee where field_serial = 5", TheConnection);

            EDBPath path = (EDBPath) command.ExecuteScalar();

            Assert.AreEqual(true, path.Open);
            Assert.AreEqual(2, path.Count);
            Assert.AreEqual(4, path[0].X);
            Assert.AreEqual(3, path[0].Y);
            Assert.AreEqual(5, path[1].X);
            Assert.AreEqual(4, path[1].Y);
        }



        [Test]
        public void TestPolygonSupport()
        {
            EDBCommand command = new EDBCommand("select field_polygon from tablee where field_serial = 6", TheConnection);

            EDBPolygon polygon = (EDBPolygon) command.ExecuteScalar();

            Assert.AreEqual(2, polygon.Count);
            Assert.AreEqual(4, polygon[0].X);
            Assert.AreEqual(3, polygon[0].Y);
            Assert.AreEqual(5, polygon[1].X);
            Assert.AreEqual(4, polygon[1].Y);
        }


        [Test]
        public void TestCircleSupport()
        {
            EDBCommand command = new EDBCommand("select field_circle from tablee where field_serial = 7", TheConnection);

            EDBCircle circle = (EDBCircle) command.ExecuteScalar();

            Assert.AreEqual(4, circle.Center.X);
            Assert.AreEqual(3, circle.Center.Y);
            Assert.AreEqual(5, circle.Radius);
        }
        
        [Test]
        public void SetParameterValueNull()
        {
            EDBCommand cmd = new EDBCommand("insert into tablef(field_bytea) values (:val)", TheConnection);
                  EDBParameter param = cmd.CreateParameter();
                  param.ParameterName="val";
            param.EDBDbType = EDBDbType.Bytea;
                  param.Value = DBNull.Value;
            
                  cmd.Parameters.Add(param);
            
                  cmd.ExecuteNonQuery();

                  cmd = new EDBCommand("select field_bytea from tablef where field_serial = (select max(field_serial) from tablef)", TheConnection);
            
                  Object result = cmd.ExecuteScalar();
            
            
            Assert.AreEqual(DBNull.Value, result);
        }
        
        
        [Test]
        public void TestCharParameterLength()
        {
            String sql = "insert into tableh(field_char5) values ( :a );" ;
    
            String aValue = "atest";
    
            EDBCommand command = new EDBCommand(sql, TheConnection);
    
            command.Parameters.Add(new EDBParameter(":a", EDBDbType.Char));
    
            command.Parameters[":a"].Value = aValue;
            command.Parameters[":a"].Size = 5;
            
            Int32 rowsAdded = command.ExecuteNonQuery();
            Assert.AreEqual(rowsAdded, 1);
            
            EDBCommand command2 = new EDBCommand("select field_char5 from tableh where field_serial = (select max(field_serial) from tableh)", TheConnection);
            
            EDBDataReader dr = command2.ExecuteReader();
            
            dr.Read();
            
            String a = dr.GetString(0);;
                        
            dr.Close();
            
            
            Assert.AreEqual(aValue, a);
        }
        
        [Test]
        public void ParameterHandlingOnQueryWithParameterPrefix()
        {
            EDBCommand command = new EDBCommand("select to_char(field_time, 'HH24:MI') from tablec where field_serial = :a;", TheConnection);
            
            EDBParameter p = new EDBParameter("a", EDBDbType.Integer);
            p.Value = 2;
            
            command.Parameters.Add(p);

            String d = (String)command.ExecuteScalar();


            Assert.AreEqual("10:03", d);
        }
        
        [Test]
        public void MultipleRefCursorSupport()
        {
            EDBCommand command = new EDBCommand("testmultcurfunc", TheConnection);
            command.CommandType = CommandType.StoredProcedure;
            
            EDBDataReader dr = command.ExecuteReader();
            
            dr.Read();
            
            Int32 one = dr.GetInt32(0);
            
            dr.NextResult();
            
            dr.Read();
            
            Int32 two = dr.GetInt32(0);
            
            dr.Close();
            
            
            Assert.AreEqual(1, one);
            Assert.AreEqual(2, two);
        }
        
        [Test]
        public void ProcedureNameWithSchemaSupport()
        {
            EDBCommand command = new EDBCommand("public.testreturnrecord", TheConnection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(new EDBParameter(":a", EDBDbType.Integer));
            command.Parameters[0].Direction = ParameterDirection.Output;

            command.Parameters.Add(new EDBParameter(":b", EDBDbType.Integer));
            command.Parameters[1].Direction = ParameterDirection.Output;

            command.ExecuteNonQuery();
            
            Assert.AreEqual(4, command.Parameters[0].Value);
            Assert.AreEqual(5, command.Parameters[1].Value);
        }
        
        [Test]
        public void ReturnRecordSupport()
        {
            EDBCommand command = new EDBCommand("testreturnrecord", TheConnection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(new EDBParameter(":a", EDBDbType.Integer));
            command.Parameters[0].Direction = ParameterDirection.Output;

            command.Parameters.Add(new EDBParameter(":b", EDBDbType.Integer));
            command.Parameters[1].Direction = ParameterDirection.Output;

            command.ExecuteNonQuery();
            
            Assert.AreEqual(4, command.Parameters[0].Value);
            Assert.AreEqual(5, command.Parameters[1].Value);
        }
        
        
        
        [Test]
        public void ParameterTypeBoolean()
        {
            EDBParameter p = new EDBParameter();
            
            p.ParameterName = "test";
            
            p.Value = true;
            
            Assert.AreEqual(EDBDbType.Boolean, p.EDBDbType);
        }

        
        [Test]
        public void ParameterTypeTimestamp()
        {
            EDBParameter p = new EDBParameter();
            
            p.ParameterName = "test";
            
            p.Value = DateTime.Now;
            
            Assert.AreEqual(EDBDbType.Timestamp, p.EDBDbType);
        }
        
        
        [Test]
        public void ParameterTypeText()
        {
            EDBParameter p = new EDBParameter();
            
            p.ParameterName = "test";
            
            p.Value = "teste";
            
            Assert.AreEqual(EDBDbType.Text, p.EDBDbType);
        }
        
        [Test]
        public void ProblemSqlInsideException()
        {
            String sql = "selec 1 as test";
            try
            {
                EDBCommand command = new EDBCommand(sql, TheConnection);
                
                command.ExecuteReader();
            }
            catch (EDBException ex)
            {
                Assert.AreEqual(sql, ex.ErrorSql);
            }
        }

        [Test]
        public void ReadUncommitedTransactionSupport()
        {
            String sql = "select 1 as test";
            
            EDBConnection c = new EDBConnection(TheConnectionString);
            
            c.Open();
            
            EDBTransaction t = c.BeginTransaction(IsolationLevel.ReadUncommitted);
            Assert.IsNotNull(t);
            
            EDBCommand command = new EDBCommand(sql, TheConnection);
                
            command.ExecuteReader().Close();
            
        }
        
        [Test]
        public void RepeatableReadTransactionSupport()
        {
            String sql = "select 1 as test";
            
            EDBConnection c = new EDBConnection(TheConnectionString);
            
            c.Open();
            
            EDBTransaction t = c.BeginTransaction(IsolationLevel.RepeatableRead);
            Assert.IsNotNull(t);
            
            EDBCommand command = new EDBCommand(sql, TheConnection);
                
            command.ExecuteReader().Close();
            
            c.Close();
            
        }
        
        [Test]
        public void SetTransactionToSerializable()
        {
            String sql = "show transaction_isolation;";
            
            EDBConnection c = new EDBConnection(TheConnectionString);
            
            c.Open();
            
            EDBTransaction t = c.BeginTransaction(IsolationLevel.Serializable);
            Assert.IsNotNull(t);
            
            EDBCommand command = new EDBCommand(sql, c);
            
            String isolation = (String)command.ExecuteScalar();
            
            c.Close();
                
            Assert.AreEqual("serializable", isolation);
        }
        
        
        [Test]
        public void TestParameterNameWithDot()
        {
            String sql = "insert into tableh(field_char5) values ( :a.parameter );" ;
    
            String aValue = "atest";
    
            EDBCommand command = new EDBCommand(sql, TheConnection);
    
            command.Parameters.Add(new EDBParameter(":a.parameter", EDBDbType.Char));
    
            command.Parameters[":a.parameter"].Value = aValue;
            command.Parameters[":a.parameter"].Size = 5;
            
            
            Int32 rowsAdded = -1;
            try
            {
                rowsAdded = command.ExecuteNonQuery();
            }
            catch (EDBException e)
            {
                Console.WriteLine(e.ErrorSql);
            }

            Assert.AreEqual(rowsAdded, 1);
            
            EDBCommand command2 = new EDBCommand("select field_char5 from tableh where field_serial = (select max(field_serial) from tableh)", TheConnection);
            
            String a = (String)command2.ExecuteScalar();
            
            Assert.AreEqual(aValue, a);
        }


        [Test]
        public void LastInsertedOidSupport()
        {

            EDBCommand insertCommand = new EDBCommand("insert into tablea(field_text) values ('a');", TheConnection);
            // Insert this dummy row, just to enable us to see what was the last oid in order we can assert it later.
            insertCommand.ExecuteNonQuery();

            EDBCommand selectCommand = new EDBCommand("select max(oid) from tablea;", TheConnection);     


            Int64 previousOid = (Int64) selectCommand.ExecuteScalar();

            insertCommand.ExecuteNonQuery();

            Assert.AreEqual(previousOid + 1, insertCommand.LastInsertedOID);

            
        }
        
        /*[Test]
        public void SetServerVersionToNull()
        {

            ServerVersion o = TheConnection.ServerVersion;
            
            if(o == null)
              return;
        }*/
        
        [Test]
        public void VerifyFunctionNameWithDeriveParameters()
        {
            try
            {
                EDBCommand invalidCommandName = new EDBCommand("invalidfunctionname", TheConnection);
                
                EDBCommandBuilder.DeriveParameters(invalidCommandName);
            }
            catch (InvalidOperationException e)
            {
                ResourceManager resman = new ResourceManager(typeof(EDBCommandBuilder));
                string expected = string.Format(resman.GetString("Exception_InvalidFunctionName"), "invalidfunctionname");
                Assert.AreEqual(expected, e.Message);
            }
        }
        
        
        [Test]
        public void DoubleSingleQuotesValueSupport()
        {
            EDBCommand command = new EDBCommand("insert into tablea(field_text) values (:a)", TheConnection);
            command.Parameters.Add(new EDBParameter("a", EDBDbType.Text));

            command.Parameters[0].Value = "''";

            Int32 rowsAdded = command.ExecuteNonQuery();

            Assert.AreEqual(1, rowsAdded);

            command.CommandText = "select * from tablea where field_text = :a";


            EDBDataReader dr = command.ExecuteReader();
            
            Assert.IsTrue(dr.Read());
            
            dr.Close();
        }
        
        [Test]
        public void ReturnInfinityDateTimeSupportNpgsqlDbType()
        {
            EDBCommand command = new EDBCommand("insert into tableb(field_timestamp) values ('infinity'::timestamp);", TheConnection);
            

            command.ExecuteNonQuery();
            
            
            command = new EDBCommand("select field_timestamp from tableb where field_serial = (select max(field_serial) from tableb);", TheConnection);
            
            Object result = command.ExecuteScalar();
            
            Assert.AreEqual(DateTime.MaxValue, result);
        }

        [Test]
        public void ReturnMinusInfinityDateTimeSupportNpgsqlDbType()
        {
            EDBCommand command = new EDBCommand("insert into tableb(field_timestamp) values ('-infinity'::timestamp);", TheConnection);
            

            command.ExecuteNonQuery();
            
            
            command = new EDBCommand("select field_timestamp from tableb where field_serial = (select max(field_serial) from tableb);", TheConnection);
            
            Object result = command.ExecuteScalar();
            
            Assert.AreEqual(DateTime.MinValue, result);
        }

        [Test]
        public void InsertInfinityDateTimeSupportNpgsqlDbType()
        {
            EDBCommand command = new EDBCommand("insert into tableb(field_timestamp) values (:a);", TheConnection);
            

            EDBParameter p = new EDBParameter("a", EDBDbType.Timestamp);

            p.Value = DateTime.MaxValue;
            
            command.Parameters.Add(p);

            command.ExecuteNonQuery();
            
            command = new EDBCommand("select field_timestamp from tableb where field_serial = (select max(field_serial) from tableb);", TheConnection);
            
            Object result = command.ExecuteScalar();
            
            Assert.AreEqual(DateTime.MaxValue, result);
        }

        [Test]
        public void InsertMinusInfinityDateTimeSupportNpgsqlDbType()
        {
            EDBCommand command = new EDBCommand("insert into tableb(field_timestamp) values (:a);", TheConnection);
            

            EDBParameter p = new EDBParameter("a", EDBDbType.Timestamp);

            p.Value = DateTime.MinValue;
            
            command.Parameters.Add(p);

            command.ExecuteNonQuery();
            
            command = new EDBCommand("select field_timestamp from tableb where field_serial = (select max(field_serial) from tableb);", TheConnection);
            
            Object result = command.ExecuteScalar();
            
            Assert.AreEqual(DateTime.MinValue, result);
        }
        
        [Test]
        public void InsertMinusInfinityDateTimeSupport()
        {
            EDBCommand command = new EDBCommand("insert into tableb(field_timestamp) values (:a);", TheConnection);
            

            EDBParameter p = new EDBParameter("a", DateTime.MinValue);

            command.Parameters.Add(p);

            command.ExecuteNonQuery();
            
            command = new EDBCommand("select field_timestamp from tableb where field_serial = (select max(field_serial) from tableb);", TheConnection);
            
            Object result = command.ExecuteScalar();
            
            Assert.AreEqual(DateTime.MinValue, result);
        }

        [Test]
        public void MinusInfinityDateTimeSupport()
        {
            EDBCommand command = TheConnection.CreateCommand();
                       
            command.Parameters.Add(new EDBParameter("p0", DateTime.MinValue));

            command.CommandText = "select 1 where current_date=:p0";
            
            Object result = command.ExecuteScalar();
            
            Assert.AreEqual(null, result);
        }
        
        
        [Test]
        public void PlusInfinityDateTimeSupport()
        {
            EDBCommand command = TheConnection.CreateCommand();
                       
            command.Parameters.Add(new EDBParameter("p0", DateTime.MaxValue));

            command.CommandText = "select 1 where current_date=:p0";
            
            Object result = command.ExecuteScalar();
            
            Assert.AreEqual(null, result);
        }


        [Test]
        public void InetTypeSupport()
        {
            EDBCommand command = new EDBCommand("insert into tablej(field_inet) values (:a);", TheConnection);
            

            EDBParameter p = new EDBParameter("a", EDBDbType.Inet);

            p.Value = new EDBInet("127.0.0.1");
            
            command.Parameters.Add(p);

            command.ExecuteNonQuery();
            
            command = new EDBCommand("select field_inet from tablej where field_serial = (select max(field_serial) from tablej);", TheConnection);
            
            Object result = command.ExecuteScalar();
            
            Assert.AreEqual(new EDBInet("127.0.0.1"), result);
        }

        [Test]
        public void IPAddressTypeSupport()
        {
            EDBCommand command = new EDBCommand("insert into tablej(field_inet) values (:a);", TheConnection);
            

            EDBParameter p = new EDBParameter("a", EDBDbType.Inet);

            p.Value = IPAddress.Parse("127.0.0.1");
            
            command.Parameters.Add(p);

            command.ExecuteNonQuery();
            
            command = new EDBCommand("select field_inet from tablej where field_serial = (select max(field_serial) from tablej);", TheConnection);
            
            Object result = command.ExecuteScalar();
            
            Assert.AreEqual(new EDBInet("127.0.0.1"), result);
        }

        [Test]
        public void BitTypeSupportWithPrepare()
        {
            EDBCommand command = new EDBCommand("insert into tablek(field_bit) values (:a);", TheConnection);
            

            EDBParameter p = new EDBParameter("a", EDBDbType.Bit);

            p.Value = true;
            
            command.Parameters.Add(p);

            command.Prepare();

            command.ExecuteNonQuery();
            
            command = new EDBCommand("select field_bit from tablek where field_serial = (select max(field_serial) from tablek);", TheConnection);
            
            Object result = command.ExecuteScalar();
            
            Assert.AreEqual(true, result);
        }

        [Test]
        public void BitTypeSupport()
        {
            EDBCommand command = new EDBCommand("insert into tablek(field_bit) values (:a);", TheConnection);
            

            EDBParameter p = new EDBParameter("a", EDBDbType.Bit);

            p.Value = true;
            
            command.Parameters.Add(p);


            command.ExecuteNonQuery();
            
            command = new EDBCommand("select field_bit from tablek where field_serial = (select max(field_serial) from tablek);", TheConnection);
            
            Object result = command.ExecuteScalar();
            
            Assert.AreEqual(true, result);
        }

        [Test]
        public void BitTypeSupport2()
        {
            EDBCommand command = new EDBCommand("insert into tablek(field_bit) values (:a);", TheConnection);
            

            EDBParameter p = new EDBParameter("a", EDBDbType.Bit);

            p.Value = 3;
            
            command.Parameters.Add(p);


            command.ExecuteNonQuery();
            
            command = new EDBCommand("select field_bit from tablek where field_serial = (select max(field_serial) from tablek);", TheConnection);
            
            Object result = command.ExecuteScalar();
            
            Assert.AreEqual(true, result);
        }


        [Test]
        public void BitTypeSupport3()
        {
            EDBCommand command = new EDBCommand("insert into tablek(field_bit) values (:a);", TheConnection);
            

            EDBParameter p = new EDBParameter("a", EDBDbType.Bit);

            p.Value = 6;
            
            command.Parameters.Add(p);


            command.ExecuteNonQuery();
            
            command = new EDBCommand("select field_bit from tablek where field_serial = (select max(field_serial) from tablek);", TheConnection);
            
            Object result = command.ExecuteScalar();
            
            Assert.AreEqual(false, result);
        }

        //[Test]
        public void FunctionReceiveCharParameter()
        {
            EDBCommand command = new EDBCommand("d/;", TheConnection);
            

            EDBParameter p = new EDBParameter("a", EDBDbType.Inet);

            p.Value = IPAddress.Parse("127.0.0.1");
            
            command.Parameters.Add(p);

            command.ExecuteNonQuery();
            
            command = new EDBCommand("select field_inet from tablej where field_serial = (select max(field_serial) from tablej);", TheConnection);
            
            Object result = command.ExecuteScalar();
            
            Assert.AreEqual(new EDBInet("127.0.0.1"), result);
        }

        [Test]
        public void FunctionCaseSensitiveName()
        {
            EDBCommand command = new EDBCommand("\"FunctionCaseSensitive\"", TheConnection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(new EDBParameter("p1", EDBDbType.Integer));
            command.Parameters.Add(new EDBParameter("p2", EDBDbType.Text));

            Object result = command.ExecuteScalar();

            Assert.AreEqual(0, result);
            
        }

        [Test]
        public void FunctionCaseSensitiveNameWithSchema()
        {
            EDBCommand command = new EDBCommand("\"public\".\"FunctionCaseSensitive\"", TheConnection);
            command.CommandType = CommandType.StoredProcedure;
            
            command.Parameters.Add(new EDBParameter("p1", EDBDbType.Integer));
            command.Parameters.Add(new EDBParameter("p2", EDBDbType.Text));
            
            Object result = command.ExecuteScalar();
            
            Assert.AreEqual(0, result);
            
        }

        [Test]
        public void FunctionCaseSensitiveNameDeriveParameters()
        {
            EDBCommand command = new EDBCommand("\"FunctionCaseSensitive\"", TheConnection);

            

            EDBCommandBuilder.DeriveParameters(command);
            
            
            Assert.AreEqual(EDBDbType.Integer, command.Parameters[0].EDBDbType);
            Assert.AreEqual(EDBDbType.Text, command.Parameters[1].EDBDbType);
            
        }
        
        [Test]
        public void FunctionCaseSensitiveNameDeriveParametersWithSchema()
        {
            
            EDBCommand command = new EDBCommand("\"public\".\"FunctionCaseSensitive\"", TheConnection);
            
            EDBCommandBuilder.DeriveParameters(command);
            
            
            Assert.AreEqual(EDBDbType.Integer, command.Parameters[0].EDBDbType);
            Assert.AreEqual(EDBDbType.Text, command.Parameters[1].EDBDbType);

            
            
        }

        [Test]
        public void FunctionTestTimestamptzParameterSupport()
        {
            
            EDBCommand command = new EDBCommand("testtimestamptzparameter", TheConnection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(new EDBParameter("p1", EDBDbType.TimestampTZ));
            
            EDBDataReader dr = command.ExecuteReader();

            Int32 count = 0;
            
            while (dr.Read())
                count++;

            Assert.IsTrue(count > 1);
            
            
            
            
            
            
        }
        
        [Test]
        public void GreaterThanInQueryStringWithPrepare()
        {
            EDBCommand command = new EDBCommand("select count(*) from tablea where field_serial >:param1", TheConnection);
            
            command.Parameters.Add(":param1", 1);
            

            command.Prepare();
            command.ExecuteScalar();
            
            
        }
        
        [Test]
        public void CharParameterValueSupport()
        {
            const String query = @"create temp table test ( tc char(1) );
            insert into test values(' ');
            select * from test where tc=:charparam";

            EDBCommand command = new EDBCommand(query, TheConnection);

            IDbDataParameter sqlParam = command.CreateParameter();
            sqlParam.ParameterName = "charparam";
            
            // Exception Can't cast System.Char into any valid DbType.
            sqlParam.Value = ' ';
            command.Parameters.Add(sqlParam);

            String res = (String)command.ExecuteScalar();
            
            Assert.AreEqual(" ", res);                    
            
            
        }
        [Test]
        public void ConnectionStringCommandTimeout()
        {
           /* NpgsqlConnection connection = new NpgsqlConnection("Server=localhost; Database=test; User=postgres; Password=12345;
CommandTimeout=180");
NpgsqlCommand command = new NpgsqlCommand("\"Foo\"", connection);
connection.Open();*/

        EDBConnection conn = new EDBConnection(TheConnectionString + ";CommandTimeout=180");
        EDBCommand command = new EDBCommand("\"Foo\"", conn);
        conn.Open();
        
        Assert.AreEqual(180, command.CommandTimeout);
            

            
            
        }
        
         [Test]
        public void ParameterExplicitType()
        {
            
            object param = 1;
            
            using(EDBCommand cmd = new EDBCommand("select a, max(b) from (select :param as a, 1 as b) x group by a", TheConnection))
            {
                cmd.Parameters.Add("param", param);
                cmd.Parameters[0].DbType = DbType.Int32;
                
                using(IDataReader rdr = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    rdr.Read();
                }

                param = "text";
                cmd.Parameters[0].DbType = DbType.String;
                using(IDataReader rdr = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    rdr.Read();
                }
            
            }
        }
        

        [Test]
        public void ParameterExplicitType2()
        {

            const string query = @"create temp table test ( tc date );  select * from test where tc=:param";
        
            EDBCommand command = new EDBCommand(query, TheConnection);

           IDbDataParameter sqlParam = command.CreateParameter();
           sqlParam.ParameterName = "param";
           sqlParam.Value = "2008-1-1";
           //sqlParam.DbType = DbType.Object;
           command.Parameters.Add(sqlParam);
           
           
           command.ExecuteScalar();
        }
        
        [Test]
        public void ParameterExplicitType2DbTypeObject()
        {

            const string query = @"create temp table test ( tc date );  select * from test where tc=:param";
        
            EDBCommand command = new EDBCommand(query, TheConnection);

           IDbDataParameter sqlParam = command.CreateParameter();
           sqlParam.ParameterName = "param";
           sqlParam.Value = "2008-1-1";
           sqlParam.DbType = DbType.Object;
           command.Parameters.Add(sqlParam);
           
           
           command.ExecuteScalar();
        }
        
        [Test]
        public void ParameterExplicitType2DbTypeObjectWithPrepare()
        {

            new EDBCommand("create temp table test ( tc date )", TheConnection).ExecuteNonQuery();
        
            const string query = @"select * from test where tc=:param";
        
            EDBCommand command = new EDBCommand(query, TheConnection);

            IDbDataParameter sqlParam = command.CreateParameter();
            sqlParam.ParameterName = "param";
            sqlParam.Value = "2008-1-1";
            sqlParam.DbType = DbType.Object;
            command.Parameters.Add(sqlParam);
           
            command.Prepare();
           
            command.ExecuteScalar();
        }
        
        [Test]
        public void ParameterExplicitType2DbTypeObjectWithPrepare2()
        {

            new EDBCommand("create temp table test ( tc date )", TheConnection).ExecuteNonQuery();
            
            const string query = @"select * from test where tc=:param or tc=:param2";
        
            EDBCommand command = new EDBCommand(query, TheConnection);

            IDbDataParameter sqlParam = command.CreateParameter();
            sqlParam.ParameterName = "param";
            sqlParam.Value = "2008-1-1";
            sqlParam.DbType = DbType.Object;
            command.Parameters.Add(sqlParam);
            
            sqlParam = command.CreateParameter();
            sqlParam.ParameterName = "param2";
            sqlParam.Value = DateTime.Now;
            sqlParam.DbType = DbType.Date;
            command.Parameters.Add(sqlParam);
            
            command.Prepare();
            
            command.ExecuteScalar();
        }

        [Test]
        public void Int32WithoutQuotesPolygon()
        {

            EDBCommand a = new EDBCommand("select 'polygon ((:a :b))' ", TheConnection);
            a.Parameters.Add(new EDBParameter("a", 1));
            a.Parameters.Add(new EDBParameter("b", 1));
            
            a.ExecuteScalar();
                      
                 
        }
        
        [Test]
        public void Int32WithoutQuotesPolygon2()
        {

            EDBCommand a = new EDBCommand("select 'polygon ((:a :b))' ", TheConnection);
            a.Parameters.Add(new EDBParameter("a", 1)).DbType = DbType.Int32;
            a.Parameters.Add(new EDBParameter("b", 1)).DbType = DbType.Int32;
            
            a.ExecuteScalar();
                      
                 
        }
        
        [Test]
        public void TestUUIDDataType()
        {

            string createTable =
            @"DROP TABLE if exists public.person;
            CREATE TABLE public.person ( 
            person_id serial PRIMARY KEY NOT NULL,
            person_uuid uuid NOT NULL
            ) WITH(OIDS=FALSE);";
            EDBCommand command = new EDBCommand(createTable, TheConnection);
            command.ExecuteNonQuery();

            string insertSql = "INSERT INTO person (person_uuid) VALUES (:param1);";
            EDBParameter uuidDbParam = new EDBParameter(":param1", EDBDbType.Uuid);
            uuidDbParam.Value = Guid.NewGuid();

            command = new EDBCommand(insertSql, TheConnection);
            command.Parameters.Add(uuidDbParam);
            command.ExecuteNonQuery();

            command = new EDBCommand("SELECT person_uuid::uuid FROM person LIMIT 1", TheConnection);


            object result = command.ExecuteScalar();
            Assert.AreEqual(typeof(Guid), result.GetType());
        }
        
        [Test]
        public void TestBug1006158OutputParameters()
        {

            string createFunction =
            @"CREATE OR REPLACE FUNCTION more_params(OUT a integer, OUT b boolean) AS
            $BODY$DECLARE
                BEGIN
                    a := 3;
                    b := true;
                END;$BODY$
              LANGUAGE 'plpgsql' VOLATILE;";
              
            EDBCommand command = new EDBCommand(createFunction, TheConnection);
            command.ExecuteNonQuery();

            command = new EDBCommand("more_params", TheConnection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(new EDBParameter("a", DbType.Int32));
            command.Parameters[0].Direction = ParameterDirection.Output;
            command.Parameters.Add(new EDBParameter("b", DbType.Boolean));
            command.Parameters[1].Direction = ParameterDirection.Output;

            Object result = command.ExecuteScalar();

            Assert.AreEqual(3, command.Parameters[0].Value);
            Assert.AreEqual(true, command.Parameters[1].Value);
        }
        
        
        [Test]
        public void TestSavePoint()
        {
            
            if (TheConnection.PostgreSqlVersion < new Version("8.0.0"))
                return;
                
            const String theSavePoint = "theSavePoint";
            
            TheTransaction.Save(theSavePoint);
            
            new EDBCommand("insert into tablea (field_text) values ('savepointtest')", TheConnection).ExecuteNonQuery();
            
            object result = new EDBCommand("select count(*) from tablea where field_text = 'savepointtest'", TheConnection).ExecuteScalar();
            
            Assert.AreEqual(1, result);
            
            TheTransaction.Rollback(theSavePoint);
            
            result = new EDBCommand("select count(*) from tablea where field_text = 'savepointtest'", TheConnection).ExecuteScalar();
            
            Assert.AreEqual(0, result);
            
        }
        
        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestSavePointWithSemicolon()
        {
            if (TheConnection.PostgreSqlVersion < new Version("8.0.0"))
                // Fake exception just to make test pass;
                throw new InvalidOperationException();
            
            const String theSavePoint = "theSavePoint;";
            
            TheTransaction.Save(theSavePoint);
            
            new EDBCommand("insert into tablea (field_text) values ('savepointtest')", TheConnection).ExecuteNonQuery();
            
            object result = new EDBCommand("select count(*) from tablea where field_text = 'savepointtest'", TheConnection).ExecuteScalar();
            
            Assert.AreEqual(1, result);
            
            TheTransaction.Rollback(theSavePoint);
            
            result = new EDBCommand("select count(*) from tablea where field_text = 'savepointtest'", TheConnection).ExecuteScalar();
            
            Assert.AreEqual(0, result);
            
        }
        
        [Test]
        public void TestPreparedStatementParameterCastIsNotAdded()
        {
            // Test by Waldemar Bergstreiser            

            new EDBCommand("create table testpreparedstatementparametercast ( C1 int );", TheConnection).ExecuteNonQuery();
            IDbCommand cmd = new EDBCommand("select C1 from testpreparedstatementparametercast where :p0 is null or  C1 = :p0 ", TheConnection);
            
            IDbDataParameter paramP0 = cmd.CreateParameter();
            paramP0.ParameterName = "p0";
            paramP0.DbType = DbType.Int32;
        cmd.Parameters.Add(paramP0);
            cmd.Prepare();    // This cause a runtime exception // Tested with PostgreSQL 8.3 //
             
            
            
        }
        
        [Test]
        [ExpectedException(typeof(EDBException))]
        public void TestErrorInPreparedStatementCausesReleaseConnectionToThrowException()
        {
            // This is caused by having an error with the prepared statement and later, Npgsql is trying to release the plan as it was successful created.             

            IDbCommand cmd = new EDBCommand("sele", TheConnection);
            
                cmd.Prepare();    
             
            
        
        }
        
        [Test]
        public void TestBug1010488ArrayParameterWithNullValue()
        {
            // Test by Christ Akkermans       
            
            new EDBCommand(@"CREATE OR REPLACE FUNCTION NullTest (input INT4[]) RETURNS VOID                             
            AS $$
            DECLARE
            BEGIN
            END
            $$ LANGUAGE plpgsql;", TheConnection).ExecuteNonQuery();
            
            using (EDBCommand cmd = new EDBCommand("NullTest", TheConnection))
            {

                EDBParameter parameter = new EDBParameter("", EDBDbType.Integer | EDBDbType.Array);
                parameter.Value = new object[] { 5, 5, DBNull.Value };
                cmd.Parameters.Add(parameter);
 
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.ExecuteNonQuery();
            }
        }

        [Test]
        public void VarCharArrayHandling()
        {
            
            using (EDBCommand cmd = new EDBCommand("select :p1", TheConnection))
            {

                EDBParameter parameter = new EDBParameter("p1", EDBDbType.Varchar | EDBDbType.Array);
                parameter.Value = new object[] { "test", "test"};
                cmd.Parameters.Add(parameter);
 
                cmd.ExecuteNonQuery();
            }
            
            
        }
        
        [Test]
        public void Bug1010521NpgsqlIntervalShouldBeQuoted()
        {
            
            using (EDBCommand cmd = new EDBCommand("select :p1", TheConnection))
            {

                EDBParameter parameter = new EDBParameter("p1", EDBDbType.Interval);
                parameter.Value = new EDBInterval(DateTime.Now.TimeOfDay);
                cmd.Parameters.Add(parameter);
 
                cmd.ExecuteNonQuery();
            }
            
            
        }

    }
    

    [TestFixture]
    public class CommandTestsV2 : CommandTests
    {
        protected override EDBConnection TheConnection {
            get { return _connV2; }
        }
        protected override EDBTransaction TheTransaction {
            get { return _tV2; }
            set { _tV2 = value; }
        }
        protected override string TheConnectionString {
            get { return _connV2String; }
        }
    }
}

