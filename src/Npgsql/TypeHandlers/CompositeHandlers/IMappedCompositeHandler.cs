using System;

namespace EnterpriseDB.EDBClient.TypeHandlers.CompositeHandlers
{
    interface IMappedCompositeHandler
    {
        /// <summary>
        /// The CLR type mapped to the PostgreSQL composite type.
        /// </summary>
        Type CompositeType { get; }
    }
}
