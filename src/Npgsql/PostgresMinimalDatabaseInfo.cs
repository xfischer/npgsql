#region License
// The PostgreSQL License
//
// Copyright (C) 2018 The EDB Development Team
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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using EnterpriseDB.EDBClient.PostgresTypes;
using EDBTypes;

namespace EnterpriseDB.EDBClient
{
    class PostgresMinimalDatabaseInfoFactory : IEDBDatabaseInfoFactory
    {
        public Task<EDBDatabaseInfo> Load(EDBConnection conn, EDBTimeout timeout, bool async)
            => Task.FromResult(
                new EDBConnectionStringBuilder(conn.ConnectionString).ServerCompatibilityMode == ServerCompatibilityMode.NoTypeLoading
                    ? (EDBDatabaseInfo)new PostgresMinimalDatabaseInfo(conn)
                    : null
            );
    }

    class PostgresMinimalDatabaseInfo : PostgresDatabaseInfo
    {
        static readonly Version DefaultVersion = new Version(10, 0);

        static readonly PostgresBaseType[] Types = typeof(EDBDbType).GetFields()
            .Select(f => f.GetCustomAttribute<BuiltInPostgresType>())
            .Where(a => a != null)
            .Select(a => new PostgresBaseType("pg_catalog", a.Name, a.OID))
            .ToArray();

        protected override IEnumerable<PostgresType> GetTypes() => Types;

        internal PostgresMinimalDatabaseInfo([NotNull] EDBConnection conn)
        {
            var csb = new EDBConnectionStringBuilder(conn.ConnectionString);
            Host = csb.Host;
            Port = csb.Port;
            Name = csb.Database;

            Version = conn.PostgresParameters.TryGetValue("server_version", out string versionString)
                ? ParseServerVersion(versionString)
                : DefaultVersion;

            HasIntegerDateTimes = !conn.PostgresParameters.TryGetValue("integer_datetimes", out var intDateTimes) ||
                                  intDateTimes == "on";
        }
    }
}
