// project created on 30/11/2002 at 22:00
//
// Author:
// 	Francisco Figueiredo Jr. <fxjrlists@yahoo.com>
//
//	Copyright (C) 2002 The Npgsql Development Team
//	npgsql-general@gborg.postgresql.org
//	http://gborg.postgresql.org/project/npgsql/projdisplay.php
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
using System.Data;
using System.Resources;
using NUnit.Framework;
using System.Collections.Generic;

namespace NpgsqlTests
{

    [TestFixture]
    public class ConnectionTests : BaseClassTests
    {
        protected override EDBConnection TheConnection {
            get { return _conn; }
        }
        protected override EDBTransaction TheTransaction {
            get { return _t; }
            set { _t = value; }
        }
        protected virtual string TheConnectionString {
            get { return _connString; }
        }
        [Test]
        public void ChangeDatabase()
        {
            TheConnection.ChangeDatabase("template1");

            EDBCommand command = new EDBCommand("select current_database()", TheConnection);

            String result = (String)command.ExecuteScalar();

            Assert.AreEqual("template1", result);
        }

		[Test]
		public void ChangeDatabaseTestConnectionCache()
		{
			EDBConnection conn1 = new EDBConnection(TheConnectionString);
			EDBConnection conn2 = new EDBConnection(TheConnectionString);

			EDBCommand command;

			//	connection 1 change database
			conn1.Open();
			conn1.ChangeDatabase("template1");
			command = new EDBCommand("select current_database()", conn1);
			string db1 = (String)command.ExecuteScalar();

			Assert.AreEqual("template1", db1);

			//	connection 2 's database should not changed, so should different from conn1
			conn2.Open();
			command = new EDBCommand("select current_database()", conn2);
			string db2 = (String)command.ExecuteScalar();

			Assert.AreNotEqual(db1, db2);

			conn1.Close();
			conn2.Close();
		}

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void NestedTransaction()
        {
            EDBTransaction t;
                
            t = TheConnection.BeginTransaction();
            if(t == null)
              return;
        }

        [Test]
        public void SequencialTransaction()
        {
            TheTransaction.Rollback();

            TheTransaction = TheConnection.BeginTransaction();
        }

        [Test]
        public void ConnectionRefused()
        {
            try
            {
                EDBConnection conn = new EDBConnection("Server=127.0.0.1;Port=44444;User Id=npgsql_tets;Password=j");

                conn.Open();
            }

            catch (EDBException e)
            {
				Type type_NpgsqlState = typeof(EDBConnection).Assembly.GetType("Npgsql.NpgsqlState");
				ResourceManager resman = new ResourceManager(type_NpgsqlState);
				string expected = string.Format(resman.GetString("Exception_FailedConnection"), "127.0.0.1");
				Assert.AreEqual(expected, e.Message);
            }
        }
		
		[Test]
        [ExpectedException(typeof(EDBException))]
        public void ConnectionStringWithEqualSignValue()
        {
            
            EDBConnection conn = new EDBConnection("Server=127.0.0.1;Port=44444;User Id=npgsql_tets;Password=j==");
            
            conn.Open();
            
        }
        
        [Test]
        [ExpectedException(typeof(EDBException))]
        public void ConnectionStringWithSemicolonSignValue()
        {
            
            EDBConnection conn = new EDBConnection("Server=127.0.0.1;Port=44444;User Id=npgsql_tets;Password='j;'");
            
            conn.Open();
            
        }
        
        [Test]
        public void SearchPathSupport()
        {
            
            EDBConnection conn = new EDBConnection(TheConnectionString + ";searchpath=public");
            conn.Open();
            
            EDBCommand c = new EDBCommand("show search_path", conn);
            
            String searchpath = (String) c.ExecuteScalar();
            //Note, public is no longer implicitly added to paths, so this is no longer "public, public".
            Assert.AreEqual("public", searchpath );
            
            
        }
        
        [Test]
        public void ConnectorNotInitializedException1000581()
        {
            
            EDBCommand command = new EDBCommand();
            command.CommandText = @"SELECT 123";

            for(int i = 0; i < 2; i++)
            {
                EDBConnection connection = new EDBConnection(TheConnectionString);
                connection.Open();
                command.Connection = connection;
                command.Transaction = connection.BeginTransaction();
                command.ExecuteScalar();
                command.Transaction.Commit();
                connection.Close();

            }
            
            
        }

        [Test]
        public void UseAllConnectionsInPool()
        {
            List<EDBConnection> openedConnections = new List<EDBConnection>();
            // repeat test to exersize pool
            for (int i = 0; i < 10; ++i)
            {
                try
                {
                    // 19 since base class opens one and the default pool size is 20
                    for (int j = 0; j < 19; ++j)
                    {
                        EDBConnection connection = new EDBConnection(TheConnectionString);
                        connection.Open();
                        openedConnections.Add(connection);
                    }
                }
                finally
                {
                    openedConnections.ForEach(delegate(EDBConnection con) { con.Dispose(); });
                    openedConnections.Clear();
                }
            }
        }

        [Test]
        [ExpectedException(typeof(Exception))]
        public void ExceedConnectionsInPool()
        {
            List<EDBConnection> openedConnections = new List<EDBConnection>();
            try
            {
                // exceed default pool size of 20
                for (int i = 0; i < 21; ++i)
                {
                    EDBConnection connection = new EDBConnection(TheConnectionString);
                    connection.Open();
                    openedConnections.Add(connection);
                }
            }
            finally
            {
                openedConnections.ForEach(delegate(EDBConnection con) { con.Dispose(); });
            }
        }

    }
    [TestFixture]
    public class ConnectionTestsV2 : ConnectionTests
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
