using System;
using System.Data.Common;
using System.Reflection;
using EnterpriseDB.EDBClient.Logging;

namespace EnterpriseDB.EDBClient
{
    /// <summary>
    /// A factory to create instances of various EnterpriseDB.EDBClient objects.
    /// </summary>
    [Serializable]
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
        public override DbCommand CreateCommand() => new EDBCommand();

        /// <summary>
        /// Returns a strongly typed <see cref="DbConnection"/> instance.
        /// </summary>
        public override DbConnection CreateConnection() => new EDBConnection();

        /// <summary>
        /// Returns a strongly typed <see cref="DbParameter"/> instance.
        /// </summary>
        public override DbParameter CreateParameter() => new EDBParameter();

        /// <summary>
        /// Returns a strongly typed <see cref="DbConnectionStringBuilder"/> instance.
        /// </summary>
        public override DbConnectionStringBuilder CreateConnectionStringBuilder() => new EDBConnectionStringBuilder();

        /// <summary>
        /// Returns a strongly typed <see cref="DbCommandBuilder"/> instance.
        /// </summary>
        public override DbCommandBuilder CreateCommandBuilder() => new EDBCommandBuilder();

        /// <summary>
        /// Returns a strongly typed <see cref="DbDataAdapter"/> instance.
        /// </summary>
        public override DbDataAdapter CreateDataAdapter() => new EDBDataAdapter();

#if !NET461 && !NET472 && !NET48 && !NETSTANDARD2_0
        /// <summary>
        /// Specifies whether the specific <see cref="DbProviderFactory"/> supports the <see cref="DbDataAdapter"/> class.
        /// </summary>
        public override bool CanCreateDataAdapter => true;

        /// <summary>
        /// Specifies whether the specific <see cref="DbProviderFactory"/> supports the <see cref="DbCommandBuilder"/> class.
        /// </summary>
        public override bool CanCreateCommandBuilder => true;
#endif

        #region IServiceProvider Members

        /// <summary>
        /// Gets the service object of the specified type.
        /// </summary>
        /// <param name="serviceType">An object that specifies the type of service object to get.</param>
        /// <returns>A service object of type serviceType, or null if there is no service object of type serviceType.</returns>

        public object? GetService(Type serviceType)
        {
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));

            // In legacy Entity Framework, this is the entry point for obtaining EnterpriseDB.EDBClient's
            // implementation of DbProviderServices. We use reflection for all types to
            // avoid any dependencies on EF stuff in this project. EF6 (and of course EF Core) do not use this method.

            if (serviceType.FullName != "System.Data.Common.DbProviderServices")
                return null;

            // User has requested a legacy EF DbProviderServices implementation. Check our cache first.
            if (_legacyEntityFrameworkServices != null)
                return _legacyEntityFrameworkServices;

            // First time, attempt to find the EntityFramework5.EnterpriseDB.EDBClient assembly and load the type via reflection
            var assemblyName = typeof(EDBFactory).GetTypeInfo().Assembly.GetName();
            assemblyName.Name = "EntityFramework5.EnterpriseDB.EDBClient";
            Assembly EDBEfAssembly;
            try {
                EDBEfAssembly = Assembly.Load(new AssemblyName(assemblyName.FullName));
            } catch {
                return null;
            }

            Type? EDBServicesType;
            if ((EDBServicesType = EDBEfAssembly.GetType("EnterpriseDB.EDBClient.EDBServices")) == null ||
                EDBServicesType.GetProperty("Instance") == null)
                throw new Exception("EntityFramework5.EnterpriseDB.EDBClient assembly does not seem to contain the correct type!");

            return _legacyEntityFrameworkServices = EDBServicesType
                .GetProperty("Instance", BindingFlags.Public | BindingFlags.Static)!
                .GetMethod!.Invoke(null, new object[0]);
        }

        static object? _legacyEntityFrameworkServices;

        #endregion
    }
}
