using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using  EnterpriseDB.EDBClient.BackendMessages;
#if NET45 || NET451
using System.Runtime.Serialization;
#endif

namespace  EnterpriseDB.EDBClient
{
    /// <summary>
    /// The exception that is thrown when server-related issues occur.
    /// </summary>
    /// <remarks>
    /// PostgreSQL errors (e.g. query SQL issues, constraint violations) are raised via
    /// <see cref="PostgresException"/> which is a subclass of this class.
    /// Purely  EnterpriseDB.EDBClient-related issues which aren't related to the server will be raised
    /// via the standard CLR exceptions (e.g. ArgumentException).
    /// </remarks>
#if NET45 || NET451
    [Serializable]
#endif
    public class EDBException : DbException
    {
        internal EDBException() {}

        internal EDBException(string message, Exception innerException) 
            : base(message, innerException) {}

        internal EDBException(string message)
            : base(message) { }

        /// <summary>
        /// Same as <see cref="PostgresException.MessageText"/>, for backwards-compatibility with  EnterpriseDB.EDBClient 2.x and Hangfire.
        /// </summary>
        /// <remarks>
        /// Until Hangfire fix themselves: https://github.com/frankhommers/Hangfire.PostgreSql/issues/33
        /// </remarks>
        [Obsolete("Use PostgresException.MessageText instead")]
        public string BaseMessage => ((PostgresException)this).MessageText;

        #region Serialization
#if NET45 || NET451
        internal EDBException(SerializationInfo info, StreamingContext context) : base(info, context) {}
#endif
        #endregion
    }
}
