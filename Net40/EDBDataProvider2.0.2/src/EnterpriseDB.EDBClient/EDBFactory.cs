// EnterpriseDB.EDBClient.EDBFactory.cs
//
// Author:
//    Francisco Jr. (fxjrlists@yahoo.com.br)
//
//    Copyright (C) 2002-2006 The EnterpriseDB.EDBClient Development Team
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

using System;
using System.Data.Common;
using System.Reflection;

// Keep the xml comment warning quiet for this file.
#pragma warning disable 1591

namespace EnterpriseDB.EDBClient
{
    /// <summary>
    /// A factory to create instances of various EnterpriseDB.EDBClient objects.
    /// </summary>
    [Serializable]
    public sealed class EDBFactory : DbProviderFactory, IServiceProvider
    {
        public static EDBFactory Instance = new EDBFactory();

        private EDBFactory()
        {
        }

        /// <summary>
        /// Creates an EDBCommand object.
        /// </summary>
        public override DbCommand CreateCommand()
        {
            return new EDBCommand();
        }

        public override DbCommandBuilder CreateCommandBuilder()
        {
            return new EDBCommandBuilder();
        }

        public override DbConnection CreateConnection()
        {
            return new EDBConnection();
        }

        public override DbDataAdapter CreateDataAdapter()
        {
            return new EDBDataAdapter();
        }

        public override DbParameter CreateParameter()
        {
            return new EDBParameter();
        }

        public override DbConnectionStringBuilder CreateConnectionStringBuilder()
        {
            return new EDBConnectionStringBuilder();
        }

        #region IServiceProvider Members

        public object GetService(Type serviceType) {
            // In legacy Entity Framework, this is the entry point for obtaining EnterpriseDB.EDBClient's
            // implementation of DbProviderServices. We use reflection for all types to
            // avoid any dependencies on EF stuff in this project.

            if (serviceType != null && serviceType.FullName == "System.Data.Common.DbProviderServices")
            {
                // User has requested a legacy EF DbProviderServices implementation. Check our cache first.
                if (_legacyEntityFrameworkServices != null)
                    return _legacyEntityFrameworkServices;

                // First time, attempt to find the EnterpriseDB.EDBClient.EntityFrameworkLegacy assembly and load the type via reflection
                var assemblyName = typeof(EDBFactory).Assembly.GetName();
                assemblyName.Name = "EnterpriseDB.EDBClient.EntityFrameworkLegacy";
                Assembly npgsqlEfAssembly;
                try {
                    npgsqlEfAssembly = Assembly.Load(assemblyName.FullName);
                } catch (Exception e) {
                    throw new Exception("Could not load EnterpriseDB.EDBClient.EntityFrameworkLegacy assembly, is it installed?", e);
                }

                Type npgsqlServicesType;
                if ((npgsqlServicesType = npgsqlEfAssembly.GetType("EnterpriseDB.EDBClient.EDBServices")) == null ||
                    npgsqlServicesType.GetProperty("Instance") == null)
                    throw new Exception("EnterpriseDB.EDBClient.EntityFrameworkLegacy assembly does not seem to contain the correct type!");

                return _legacyEntityFrameworkServices = npgsqlServicesType.InvokeMember("Instance", BindingFlags.Public | BindingFlags.Static | BindingFlags.GetProperty, null, null, new object[0]);
            }

#pragma warning disable CS8603 // Possible null reference return.
            return null;
#pragma warning restore CS8603 // Possible null reference return.
        }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private static object _legacyEntityFrameworkServices;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        #endregion
    }
}

#pragma warning restore 1591
