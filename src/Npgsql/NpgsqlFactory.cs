#region License
// The PostgreSQL License
//
// Copyright (C) 2016 The  EnterpriseDB.EDBClient Development Team
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

// Keep the xml comment warning quiet for this file.
#pragma warning disable 1591

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

        public override DbConnection CreateConnection()
        {
            return new EDBConnection();
        }


        public override DbParameter CreateParameter()
        {
            return new EDBParameter();
        }

        public override DbConnectionStringBuilder CreateConnectionStringBuilder()
        {
            return new EDBConnectionStringBuilder();
        }

#if NET45 || NET451
        public override DbCommandBuilder CreateCommandBuilder()
        {
            return new EDBCommandBuilder();
        }

        public override DbDataAdapter CreateDataAdapter()
        {
            return new EDBDataAdapter();
        }
#endif

        #region IServiceProvider Members

        public object GetService(Type serviceType) {

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
                assemblyName.Name = "EntityFramework5.EnterpriseDB.EDBClient";
                Assembly npgsqlEfAssembly;
                try {
                    npgsqlEfAssembly = Assembly.Load(new AssemblyName(assemblyName.FullName));
                   
                } catch (Exception e) {
                    throw new Exception("Could not load EntityFramework5-----V6.EnterpriseDB.EDBClient assembly, is it installed?",e);
                }
                
                Type npgsqlServicesType;
                if ((npgsqlServicesType = npgsqlEfAssembly.GetType("EnterpriseDB.EDBClient.EDBServices")) == null ) 
                    throw new Exception("EntityFramework5.EnterpriseDB.EDBClient assembly does not seem to contain the correct type-- NULL EnterprirDEB.EDBServices!");
               if( npgsqlServicesType.GetProperty("Instance") == null)
                    throw new Exception("EntityFramework5.EnterpriseDB.EDBClient assembly does not seem to contain the correct type-- GetProperty(Instance) is NULL !");
                return _legacyEntityFrameworkServices = npgsqlServicesType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static).GetMethod.Invoke(null, new object[0]);
            }

            return null;
        }

        private static object _legacyEntityFrameworkServices;

        #endregion
    }
}

#pragma warning restore 1591
