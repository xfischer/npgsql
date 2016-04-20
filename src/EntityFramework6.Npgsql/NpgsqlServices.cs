#region License
// The PostgreSQL License
//
// Copyright (C) 2015 The Npgsql Development Team
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
//
// IN NO EVENT SHALL THE NPGSQL DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE NPGSQL DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// THE NPGSQL DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE NPGSQL DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.
#endregion

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Xml;
#if ENTITIES6
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Migrations.Sql;
using System.Data.Entity.Infrastructure.DependencyResolution;

#else
using System.Data.Common.CommandTrees;
using System.Data.Common;
using System.Data.Metadata.Edm;
#endif
using EnterpriseDB.EDBClient.SqlGenerators;

namespace EnterpriseDB.EDBClient
{
#if ENTITIES6
    public class  EDBServices : DbProviderServices
#else
    internal class EDBServices : DbProviderServices
#endif
    {
        private static readonly EDBServices _instance = new EDBServices();

#if ENTITIES6
        public EDBServices()
        {
            AddDependencyResolver(new SingletonDependencyResolver<Func<MigrationSqlGenerator>>(
                () => new EDBMigrationSqlGenerator(), "EnterpriseDB.EDBClient"));
        }
#endif

        public static EDBServices Instance
        {
            get { return _instance; }
        }

        protected override DbCommandDefinition CreateDbCommandDefinition(DbProviderManifest providerManifest, DbCommandTree commandTree)
        {
            return CreateCommandDefinition(CreateDbCommand(commandTree));
        }

        internal DbCommand CreateDbCommand(DbCommandTree commandTree)
        {
            if (commandTree == null)
                throw new ArgumentNullException("commandTree");

            DbCommand command = EDBFactory.Instance.CreateCommand();

            foreach (KeyValuePair<string, TypeUsage> parameter in commandTree.Parameters)
            {
                DbParameter dbParameter = command.CreateParameter();
                dbParameter.ParameterName = parameter.Key;
                dbParameter.DbType = EDBProviderManifest.GetDbType(((PrimitiveType)parameter.Value.EdmType).PrimitiveTypeKind);
                command.Parameters.Add(dbParameter);
            }

            TranslateCommandTree(commandTree, command);

            return command;
        }

        private void TranslateCommandTree(DbCommandTree commandTree, DbCommand command)
        {
            SqlBaseGenerator sqlGenerator = null;

            DbQueryCommandTree select;
            DbInsertCommandTree insert;
            DbUpdateCommandTree update;
            DbDeleteCommandTree delete;
            if ((select = commandTree as DbQueryCommandTree) != null)
            {
                sqlGenerator = new SqlSelectGenerator(select);
            }
            else if ((insert = commandTree as DbInsertCommandTree) != null)
            {
                sqlGenerator = new SqlInsertGenerator(insert);
            }
            else if ((update = commandTree as DbUpdateCommandTree) != null)
            {
                sqlGenerator = new SqlUpdateGenerator(update);
            }
            else if ((delete = commandTree as DbDeleteCommandTree) != null)
            {
                sqlGenerator = new SqlDeleteGenerator(delete);
            }
            else
            {
                // TODO: get a message (unsupported DbCommandTree type)
                throw new ArgumentException();
            }

            sqlGenerator.BuildCommand(command);
        }

        protected override string GetDbProviderManifestToken(DbConnection connection)
        {
            if (connection == null)
                throw new ArgumentNullException("connection");
            string serverVersion = "";
            UsingPostgresDBConnection((EDBConnection)connection, conn =>
            {
                serverVersion = conn.ServerVersion;
            });
            return serverVersion;
        }

        protected override DbProviderManifest GetDbProviderManifest(string versionHint)
        {
            if (versionHint == null)
                throw new ArgumentNullException("versionHint");
            return new EDBProviderManifest(versionHint);
        }

#if ENTITIES6
        protected override bool DbDatabaseExists(DbConnection connection, int? commandTimeout, StoreItemCollection storeItemCollection)
        {
            bool exists = false;
            UsingPostgresDBConnection((EDBConnection)connection, conn =>
            {
                using (EDBCommand command = new EDBCommand("select count(*) from pg_catalog.pg_database where datname = '" + connection.Database + "';", conn))
                {
                    exists = Convert.ToInt32(command.ExecuteScalar()) > 0;
                }
            });
            return exists;
        }

        protected override void DbCreateDatabase(DbConnection connection, int? commandTimeout, StoreItemCollection storeItemCollection)
        {
            UsingPostgresDBConnection((EDBConnection)connection, conn =>
            {
                var sb = new StringBuilder();
                sb.Append("CREATE DATABASE \"");
                sb.Append(connection.Database);
                sb.Append("\"");
                
                if (conn.EntityTemplateDatabase != null)
                {
                    sb.Append(" TEMPLATE \"");
                    sb.Append(conn.EntityTemplateDatabase);
                    sb.Append("\"");
                }

                using (EDBCommand command = new EDBCommand(sb.ToString(), conn))
                {
                    command.ExecuteNonQuery();
                }
            });
        }

        protected override void DbDeleteDatabase(DbConnection connection, int? commandTimeout, StoreItemCollection storeItemCollection)
        {
            UsingPostgresDBConnection((EDBConnection)connection, conn =>
            {
                //Close all connections in pool or exception "database used by another user appears"
                EDBConnection.ClearAllPools();
                using (EDBCommand command = new EDBCommand("DROP DATABASE \"" + connection.Database + "\";", conn))
                {
                    command.ExecuteNonQuery();
                }
            });
        }
#endif

        private static void UsingPostgresDBConnection(EDBConnection connection, Action<EDBConnection> action)
        {
            var connectionBuilder = new EDBConnectionStringBuilder(connection.ConnectionString)
            {
                Database = "template1",
                Pooling = false
            };

            using (var masterConnection = new EDBConnection(connectionBuilder.ConnectionString))
            {
                masterConnection.Open();//using's Dispose will close it even if exception...
                action(masterConnection);
            }
        }
    }
}
