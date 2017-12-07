#region License
// The PostgreSQL License
//
// Copyright (C) 2017 The  EnterpriseDB.EDBClient DEVELOPMENT Team
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
//
// IN NO EVENT SHALL THE  EnterpriseDB.EDBClient DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE  EnterpriseDB.EDBClient DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// THE  EnterpriseDB.EDBClient DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE  EnterpriseDB.EDBClient DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.
#endregion

using System;
using System.Data.Common;
using System.Reflection;
using JetBrains.Annotations;

namespace  EnterpriseDB.EDBClient
{
    /// <summary>
    /// A factory to create instances of various  EnterpriseDB.EDBClient objects.
    /// </summary>
#if NET45 || NET451
    [Serializable]
#endif
    public sealed class EDBFactory : DbProviderFactory, IServiceProvider
    {
        /// <summary>
        /// Gets an instance of the <see cref="EDBFactory"/>.
        /// This can be used to retrieve strongly typed data objects.
        /// </summary>
        public static readonly EDBFactory Instance = new EDBFactory();

        EDBFactory() {}

        /// <summary>
        /// Returns a strongly typed <see cref="DbCommand"/> instance.
        /// </summary>
        [NotNull] public override DbCommand CreateCommand() => new EDBCommand();

        /// <summary>
        /// Returns a strongly typed <see cref="DbConnection"/> instance.
        /// </summary>
        [NotNull] public override DbConnection CreateConnection() => new EDBConnection();

        /// <summary>
        /// Returns a strongly typed <see cref="DbParameter"/> instance.
        /// </summary>
        [NotNull] public override DbParameter CreateParameter() => new EDBParameter();

        /// <summary>
        /// Returns a strongly typed <see cref="DbConnectionStringBuilder"/> instance.
        /// </summary>
        [NotNull] public override DbConnectionStringBuilder CreateConnectionStringBuilder() => new EDBConnectionStringBuilder();

#if NET45 || NET451
        /// <summary>
        /// Returns a strongly typed <see cref="DbCommandBuilder"/> instance.
        /// </summary>
        [NotNull] public override DbCommandBuilder CreateCommandBuilder() => new EDBCommandBuilder();

        /// <summary>
        /// Returns a strongly typed <see cref="DbDataAdapter"/> instance.
        /// </summary>
        [NotNull] public override DbDataAdapter CreateDataAdapter() => new EDBDataAdapter();
#endif

        #region IServiceProvider Members

        /// <summary>
        /// Gets the service object of the specified type.
        /// </summary>
        /// <param name="serviceType">An object that specifies the type of service object to get.</param>
        /// <returns>A service object of type serviceType, or null if there is no service object of type serviceType.</returns>
        [CanBeNull]
        public object GetService([NotNull] Type serviceType)
        {
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));

            // In legacy Entity Framework, this is the entry point for obtaining  EnterpriseDB.EDBClient's
            // implementation of DbProviderServices. We use reflection for all types to
            // avoid any dependencies on EF stuff in this project.

            if (serviceType.FullName == "System.Data.Common.DbProviderServices")
            {
                // User has requested a legacy EF DbProviderServices implementation. Check our cache first.
                if (_legacyEntityFrameworkServices != null)
                    return _legacyEntityFrameworkServices;

                // First time, attempt to find the EntityFramework5. EnterpriseDB.EDBClient assembly and load the type via reflection
                var assemblyName = typeof(EDBFactory).GetTypeInfo().Assembly.GetName();
                assemblyName.Name = "EntityFramework5. EnterpriseDB.EDBClient";
                Assembly npgsqlEfAssembly;
                try {
                    npgsqlEfAssembly = Assembly.Load(new AssemblyName(assemblyName.FullName));
                } catch (Exception e) {
                    throw new Exception("Could not load EntityFramework5. EnterpriseDB.EDBClient assembly, is it installed?", e);
                }

                Type npgsqlServicesType;
                if((npgsqlServicesType = npgsqlEfAssembly.GetType("EnterpriseDB.EDBClient.EDBServices")) == null)
                    throw new Exception("EntityFramework5.EnterpriseDB.EDBClient assembly does not seem to contain the correct type-- NULL EnterprirDEB.EDBServices!");
                if (npgsqlServicesType.GetProperty("Instance") == null)
                    throw new Exception("EntityFramework5.EnterpriseDB.EDBClient assembly does not seem to contain the correct type-- GetProperty(Instance) is NULL !");

                return _legacyEntityFrameworkServices = npgsqlServicesType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static).GetMethod.Invoke(null, new object[0]);
            }

            return null;
        }

        [CanBeNull]
        static object _legacyEntityFrameworkServices;

        #endregion
    }
}
