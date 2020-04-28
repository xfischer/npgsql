using System;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;

namespace EnterpriseDB.EDBClient
{
    /// <summary>
    /// Use this class in LINQ queries to emit type manipulation SQL fragments.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedParameter.Global")]
    public static class EDBTypeFunctions
    {
        /// <summary>
        /// Emits an explicit cast for unknown types sent as strings to their correct postgresql type.
        /// </summary>
        [DbFunction("EnterpriseDB.EDBClient", "cast")]
        public static string Cast(string unknownTypeValue, string postgresTypeName)
            => throw new NotSupportedException();
    }
}
